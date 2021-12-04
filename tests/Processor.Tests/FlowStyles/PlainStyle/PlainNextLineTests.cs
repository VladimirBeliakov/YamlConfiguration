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
	public class PlainNextLineTests : PlainBaseTest
	{
		[TestCaseSource(nameof(getNotFlowContexts))]
		public void GetPatternFor_NotFlowContext_Throws(Context context)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Plain.NextLine.GetPatternFor(context));
		}

		[TestCaseSource(nameof(getPositiveTestCases), new Object[] { Context.FlowIn })]
		public void GetPatternFor_ValidNextPlainLineInFlowIn_Matches(RegexTestCase testCase)
		{
			var match = _flowInNextLineRegex.Match(testCase.TestValue);

			Assert.That(match.Groups.Count, Is.EqualTo(2));
			Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
			Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.WholeMatch));
		}

		[TestCaseSource(nameof(getPositiveTestCases), new Object[] { Context.FlowOut })]
		public void GetPatternFor_ValidNextPlainLineInFlowOut_Matches(RegexTestCase testCase)
		{
			var match = _flowOutNextLineRegex.Match(testCase.TestValue);

			Assert.That(match.Groups.Count, Is.EqualTo(2));
			Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
			Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.WholeMatch));
		}

		[TestCaseSource(nameof(getNegativeTextCases), new Object[] { Context.FlowIn })]
		public void GetPatternFor_InvalidNextPlainLineInFlowIn_DoesNotMatch(string testCase)
		{
			Assert.False(_flowInNextLineRegex.IsMatch(testCase));
		}

		[TestCaseSource(nameof(getNegativeTextCases), new Object[] { Context.FlowOut })]
		public void GetPatternFor_InvalidNextPlainLineInFlowKey_DoesNotMatch(string testCase)
		{
			Assert.False(_flowOutNextLineRegex.IsMatch(testCase));
		}

		private static IEnumerable<RegexTestCase> getPositiveTestCases(Context context)
		{
			foreach (var nsPlainSafeChars in GetNsPlainSafeCharGroups(context))
				foreach (var plainNextLine in getPlainNextLine(nsPlainSafeChars))
					yield return new(
						testValue: plainNextLine + BasicStructures.Break,
						wholeMatch: plainNextLine
					);
		}

		private static IEnumerable<string> getPlainNextLine(IReadOnlyCollection<string> nsPlainSafeChars)
		{
			const string mappingValue = ":";
			const string comment = "#";
			const string tab = "\t";
			const string space = " ";

			var nsPlainChars = nsPlainSafeChars.Except(new[] { mappingValue, comment }).ToList();

			const int groupItemCount = Characters.CharGroupMaxLength;
			const int whiteCharGroupCount = groupItemCount / 2;

			var anyNsPlainChar = nsPlainChars.First();

			var anyNsPlainCharLength = anyNsPlainChar.Length;
			var nsPlainCharGroupLength = anyNsPlainCharLength * groupItemCount / 2;
			var oneGroupLength = anyNsPlainCharLength + whiteCharGroupCount + nsPlainCharGroupLength;

			var sb = new StringBuilder(oneGroupLength);

			sb.Append(anyNsPlainChar);

			var isEvenIteration = false;
			foreach (var nsPlainChar in nsPlainChars)
			{
				if (nsPlainChar.Length != anyNsPlainCharLength)
					throw new InvalidOperationException(
						$"All value lengths of {nameof(nsPlainChars)} must be equal to each other."
					);

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
					sb.Append(anyNsPlainChar);
				}
			}

			if (sb.Length > 0)
				yield return sb.ToString();

			yield return anyNsPlainChar + Helpers.RepeatAndJoin(
				CharStore.SpacesAndTabs + anyNsPlainChar,
				groupItemCount
			);

			var anyNsPlainCharGroup = String.Join(String.Empty, nsPlainChars.Take(groupItemCount));

			yield return anyNsPlainChar + anyNsPlainCharGroup;

			var anyNsPlainSafe = nsPlainSafeChars.First();

			yield return anyNsPlainChar + comment + comment;
			yield return mappingValue + mappingValue + anyNsPlainSafe;
		}

		private static IEnumerable<string> getNegativeTextCases(Context context)
		{
			const string nsChar = "a";
			const string whiteChar = " ";
			const string invalidNsChar = whiteChar;
			const string nsPlainChar = nsChar;
			const string comment = "#";
			const string mappingValue = ":";

			IReadOnlyCollection<string> invalidNsPlainSafes = context switch
			{
				Context.FlowOut => new[] { " " },
				Context.FlowIn => CharStore.FlowIndicators.ToList(),
				_ => throw new ArgumentOutOfRangeException(
					nameof(context),
					context,
					$"Only {Context.FlowIn} and {Context.FlowOut} can be processed."
				)
			};

			// Too many white chars
			var tooManyWhiteChars = CharStore.SpacesAndTabs + " ";
			var nsPlainInLine = tooManyWhiteChars + nsPlainChar;
			yield return nsPlainChar + nsPlainInLine;

			// Too many ns plain chars
			var tooManyNsPlainChars = Helpers.RepeatAndJoin(nsPlainChar, Characters.CharGroupMaxLength + 1);
			yield return nsPlainChar + tooManyNsPlainChars;

			// Too long ns plain in line
			var tooLongNsPlainInLine =
				Helpers.RepeatAndJoin(whiteChar + nsPlainChar, Characters.CharGroupMaxLength) + whiteChar;
			yield return nsPlainChar + tooLongNsPlainInLine;

			// Ns plain in line with a white space but without ns plain char
			const string invalidNsPlainInLine = whiteChar;
			yield return nsPlainChar + invalidNsPlainInLine;

			// Invalid ns plain char
			yield return nsPlainChar + invalidNsChar + comment;

			foreach (var invalidNsPlainSafe in invalidNsPlainSafes)
			{
				yield return nsPlainChar + invalidNsPlainSafe;
				yield return nsPlainChar + mappingValue + invalidNsPlainSafe;
			}
		}

		private static IEnumerable<Context> getNotFlowContexts() =>
			Enum.GetValues<Context>().Where(c => c is not Context.FlowIn and not Context.FlowOut);

		private static readonly Regex _flowInNextLineRegex =
			new(Plain.NextLine.GetPatternFor(Context.FlowIn), RegexOptions.Compiled);

		private static readonly Regex _flowOutNextLineRegex =
			new(Plain.NextLine.GetPatternFor(Context.FlowOut), RegexOptions.Compiled);
	}
}
