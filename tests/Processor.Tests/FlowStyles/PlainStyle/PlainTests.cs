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
	public class PlainTests
	{
		[TestCaseSource(nameof(getPositiveTestCases), new object[] { Context.BlockKey })]
		public void ValidOnePlainLineInBlockKey_Matches(RegexTestCase testCase)
		{
			var match = _blockKeyOneLineRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
		}

		[TestCaseSource(nameof(getPositiveTestCases), new object[] { Context.FlowKey })]
		public void ValidOnePlainLineInFlowKey_Matches(RegexTestCase testCase)
		{
			var match = _flowKeyOneLineRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
		}

		[TestCaseSource(nameof(getNegativeTextCases))]
		public void InvalidOnePlainLineInBlockKey_DoesNotMatch(string testCase)
		{
			Assert.False(_blockKeyOneLineRegex.IsMatch(testCase));
		}

		[TestCaseSource(nameof(getNegativeTextCases))]
		public void InvalidOnePlainLineInFlowKey_DoesNotMatch(string testCase)
		{
			Assert.False(_flowKeyOneLineRegex.IsMatch(testCase));
		}

		private static IEnumerable<RegexTestCase> getPositiveTestCases(Context context)
		{
			var excludedChars = context switch
			{
				Context.BlockKey => Enumerable.Empty<string>(),
				Context.FlowKey => CharStore.FlowIndicators,
				_ => throw new ArgumentOutOfRangeException(
					nameof(context),
					context,
					$"Only {Context.BlockKey} and {Context.FlowKey} can be processed."
				)
			};

			var nsPlainSafeCharsWithoutSurrogates =
				CharStore.GetNsCharsWithoutSurrogates().Except(excludedChars).ToList();

			var nsPlainSafeSurrogates = CharStore.SurrogatePairs.Value;
			var whiteChars = CharStore.SpacesAndTabs;

			foreach (var nsPlainSafeChars in new[] { nsPlainSafeCharsWithoutSurrogates, nsPlainSafeSurrogates })
			foreach (var nbNsPlainInLine in createNbNsPlainInLineFrom(nsPlainSafeChars))
				yield return new RegexTestCase(
					testValue: nbNsPlainInLine + whiteChars + ":",
					wholeMatch: nbNsPlainInLine
				);
		}

		private static IEnumerable<string> createNbNsPlainInLineFrom(IReadOnlyCollection<string> nsPlainSafeChars)
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

			var nsPlainFirstCount = nsPlainChars.Count / (groupItemCount / 4) + 1;
			var nsPlainFirsts = new List<string>(nsPlainFirstCount);

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

			var anyNsPlainCharGroup = string.Join(string.Empty, nsPlainChars.Take(groupItemCount));

			yield return anyNsPlainFirst + anyNsPlainCharGroup;

			foreach (var nsPlainFirst in nsPlainFirsts)
				yield return nsPlainFirst;

			var anyNsPlainSafe = nsPlainSafeChars.First();

			foreach (var nsPlainFirst in new[] { mappingKey, mappingValue, sequenceEntry })
				yield return nsPlainFirst + anyNsPlainSafe;

			yield return anyNsPlainFirst + comment + comment;
			yield return mappingValue + mappingValue + anyNsPlainSafe;
		}

		private static IEnumerable<string> getNegativeTextCases()
		{
			const string whiteChar = " ";
			const string mappingKey = "?";
			const string mappingValue = ":";
			const string sequenceEntry = "-";
			const string nsPlainSafe = "a";

			// Invalid ns plain first
			var conditionalNsPlainFirsts = new[] { mappingKey, mappingValue, sequenceEntry };

			foreach (var inValidNsPlainFirst in CharStore.CIndicators.Except(conditionalNsPlainFirsts))
				yield return inValidNsPlainFirst + nsPlainSafe;

			foreach (var invalidNsPlainFirst in conditionalNsPlainFirsts)
				yield return invalidNsPlainFirst + whiteChar + nsPlainSafe;
		}

		private static readonly Regex _blockKeyOneLineRegex =
			new(Plain.GetPatternFor(Context.BlockKey), RegexOptions.Compiled);

		private static readonly Regex _flowKeyOneLineRegex =
			new(Plain.GetPatternFor(Context.FlowKey), RegexOptions.Compiled);
	}
}