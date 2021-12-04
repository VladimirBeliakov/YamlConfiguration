using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class PlainTests : PlainBaseTest
	{
		[TestCaseSource(nameof(getPositiveTestCases), new Object[] { Context.BlockKey })]
		public void ValidOnePlainLineInBlockKey_Matches(RegexTestCase testCase)
		{
			var match = _blockKeyOneLineRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
		}

		[TestCaseSource(nameof(getPositiveTestCases), new Object[] { Context.FlowKey })]
		public void ValidOnePlainLineInFlowKey_Matches(RegexTestCase testCase)
		{
			var match = _flowKeyOneLineRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
		}

		[TestCaseSource(nameof(getNegativeTextCases), new Object[] { Context.BlockKey })]
		public void InvalidOnePlainLineInBlockKey_DoesNotMatch(string testCase)
		{
			Assert.False(_blockKeyOneLineRegexWithAnchorAtEnd.IsMatch(testCase));
		}

		[TestCaseSource(nameof(getNegativeTextCases), new Object[] { Context.FlowKey })]
		public void InvalidOnePlainLineInFlowKey_DoesNotMatch(string testCase)
		{
			Assert.False(_flowKeyOneLineRegexWithAnchorAtEnd.IsMatch(testCase));
		}

		private static IEnumerable<RegexTestCase> getPositiveTestCases(Context context)
		{
			var whiteChars = CharStore.SpacesAndTabs;

			foreach (var nsPlainSafeChars in GetNsPlainSafeCharGroups(context))
				foreach (var nsPlainOneLine in createNsPlainOneLinesFrom(nsPlainSafeChars))
					yield return new RegexTestCase(
						testValue: nsPlainOneLine + whiteChars + ":",
						wholeMatch: nsPlainOneLine
					);
		}

		private static IEnumerable<string> createNsPlainOneLinesFrom(
			IReadOnlyCollection<string> nsPlainSafeChars
		)
		{
			static bool canBeNsPlainFirst(string nsChar) => !CharStore.CIndicators.Contains(nsChar);

			const string mappingValue = ":";
			const string mappingKey = "?";
			const string sequenceEntry = "-";
			const string comment = "#";
			const string tab = "\t";
			const string space = " ";

			var nsPlainChars = nsPlainSafeChars.Except(new[] { mappingValue, comment }).ToList();

			const int groupItemCount = Characters.CharGroupMaxLength;
			const int whiteCharGroupCount = groupItemCount / 2;

			var anyNsPlainFirst = nsPlainChars.First(canBeNsPlainFirst);
			var anyNsPlainChar = anyNsPlainFirst;

			var anyNsPlainCharLength = anyNsPlainChar.Length;
			var nsPlainCharGroupLength = anyNsPlainCharLength * groupItemCount / 2;
			var oneGroupLength = anyNsPlainCharLength + whiteCharGroupCount + nsPlainCharGroupLength;

			var sb = new StringBuilder(oneGroupLength);

			var nsPlainFirsts = new List<string>();

			sb.Append(anyNsPlainFirst);

			var isEvenIteration = false;
			var wasNsPlainFirstCollected = false;
			foreach (var nsPlainChar in nsPlainChars)
			{
				if (nsPlainChar.Length != anyNsPlainCharLength)
					throw new InvalidOperationException(
						$"All value lengths of {nameof(nsPlainChars)} must be equal to each other."
					);

				if (!wasNsPlainFirstCollected && canBeNsPlainFirst(nsPlainChar))
				{
					nsPlainFirsts.Add(nsPlainChar);
					wasNsPlainFirstCollected = true;
				}

				var whiteChar = isEvenIteration ? tab : space;

				sb.Append(whiteChar);
				sb.Append(mappingValue);
				sb.Append(nsPlainChar);
				sb.Append(comment);

				isEvenIteration = !isEvenIteration;

				if (oneGroupLength == sb.Length)
				{
					yield return sb.ToString();
					sb.Clear();
					sb.Append(anyNsPlainFirst);
					wasNsPlainFirstCollected = false;
				}
			}

			if (sb.Length > 0)
				yield return sb.ToString();

			yield return anyNsPlainFirst + Helpers.RepeatAndJoin(
				CharStore.SpacesAndTabs + anyNsPlainChar,
				groupItemCount
			);

			var anyNsPlainCharGroup = String.Join(String.Empty, nsPlainChars.Take(groupItemCount));

			yield return anyNsPlainFirst + anyNsPlainCharGroup;

			foreach (var nsPlainFirst in nsPlainFirsts)
				yield return nsPlainFirst;

			var anyNsPlainSafe = nsPlainSafeChars.First();

			foreach (var nsPlainFirst in new[] { mappingKey, mappingValue, sequenceEntry })
				yield return nsPlainFirst + anyNsPlainSafe;

			yield return anyNsPlainFirst + comment + comment;
			yield return mappingValue + mappingValue + anyNsPlainSafe;
		}

		private static IEnumerable<string> getNegativeTextCases(Context context)
		{
			const string nsChar = "a";
			const string whiteChar = " ";
			const string invalidNsChar = whiteChar;
			const string nsPlainFirst = nsChar;
			const string nsPlainChar = nsChar;
			const string comment = "#";
			const string mappingKey = "?";
			const string mappingValue = ":";
			const string sequenceEntry = "-";
			const string nsPlainSafe = "a";

			IReadOnlyCollection<string> invalidNsPlainSafes = context switch
			{
				Context.BlockKey => new[] { " " },
				Context.FlowKey => CharStore.FlowIndicators.ToList(),
				_ => throw new ArgumentOutOfRangeException(
					nameof(context),
					context,
					$"Only {Context.BlockKey} and {Context.FlowKey} can be processed."
				)
			};

			// Invalid ns plain first
			var conditionalNsPlainFirsts = new[] { mappingKey, mappingValue, sequenceEntry };

			foreach (var inValidNsPlainFirst in CharStore.CIndicators.Except(conditionalNsPlainFirsts))
				yield return inValidNsPlainFirst + nsPlainSafe;

			foreach (var invalidNsPlainFirst in conditionalNsPlainFirsts)
				yield return invalidNsPlainFirst + whiteChar + nsPlainSafe;

			// Too many white chars
			var tooManyWhiteChars = CharStore.SpacesAndTabs + " ";
			var nsPlainInLine = tooManyWhiteChars + nsPlainChar;
			yield return nsPlainFirst + nsPlainInLine;

			// Too many ns plain chars
			var tooManyNsPlainChars = Helpers.RepeatAndJoin(nsPlainChar, Characters.CharGroupMaxLength + 1);
			yield return nsPlainFirst + tooManyNsPlainChars;

			// Too long ns plain in line
			var tooLongNsPlainInLine =
				Helpers.RepeatAndJoin(whiteChar + nsPlainChar, Characters.CharGroupMaxLength) + whiteChar;
			yield return nsPlainFirst + tooLongNsPlainInLine;

			// Ns plain in line with a white space but without ns plain char
			const string invalidNsPlainInLine = whiteChar;
			yield return nsPlainFirst + invalidNsPlainInLine;

			// Invalid ns plain char
			yield return nsPlainFirst + invalidNsChar + comment;

			foreach (var invalidNsPlainSafe in invalidNsPlainSafes)
			{
				yield return nsPlainFirst + invalidNsPlainSafe;
				yield return nsPlainFirst + mappingValue + invalidNsPlainSafe;
			}
		}

		private static Regex getRegexPatternFor(Context context, bool withAnchorAtEnd = false)
		{
			var regexPattern = Plain.GetPatternFor(context);

			if (withAnchorAtEnd)
				regexPattern = regexPattern.WithAnchorAtEnd();

			return new(regexPattern, RegexOptions.Compiled);
		}

		private static readonly Regex _blockKeyOneLineRegex =
			getRegexPatternFor(Context.BlockKey);

		private static readonly Regex _flowKeyOneLineRegex =
			getRegexPatternFor(Context.FlowKey);

		private static readonly Regex _blockKeyOneLineRegexWithAnchorAtEnd =
			getRegexPatternFor(Context.BlockKey, withAnchorAtEnd: true);

		private static readonly Regex _flowKeyOneLineRegexWithAnchorAtEnd =
			getRegexPatternFor(Context.FlowKey, withAnchorAtEnd: true);
	}
}
