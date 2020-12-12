using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Processor;
using Processor.FlowStyles;
using Processor.TypeDefinitions;

namespace ProcessorTests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class PlainOneLineTests
	{
		[TestCaseSource(nameof(getPositiveTestCases), new object[] { BlockFlow.BlockKey })]
		public void ValidOnePlainLineInBlockKey_ReturnsTrueAndExtractedValue(string testValue)
		{
			var isSuccess = PlainStyle.IsOneLine(testValue, BlockFlow.BlockKey);

			Assert.True(isSuccess);
		}

		[TestCaseSource(nameof(getPositiveTestCases), new object[] { BlockFlow.FlowKey })]
		public void ValidOnePlainLineInFlowKey_ReturnsTrueAndExtractedValue(string testValue)
		{
			var isSuccess = PlainStyle.IsOneLine(testValue, BlockFlow.FlowKey);

			Assert.True(isSuccess);
		}

		[TestCaseSource(nameof(getNegativeTextCases), new object[] { BlockFlow.BlockKey })]
		public void InvalidOnePlainLineInBlockKey_ReturnsFalse(string testCase)
		{
			var result = PlainStyle.IsOneLine(testCase, BlockFlow.BlockKey);

			Assert.False(result);
		}

		[TestCaseSource(nameof(getNegativeTextCases), new object[] { BlockFlow.FlowKey })]
		public void InvalidOnePlainLineInFlowKey_ReturnsFalse(string testCase)
		{
			var result = PlainStyle.IsOneLine(testCase, BlockFlow.FlowKey);

			Assert.False(result);
		}

		private static IEnumerable<string> getPositiveTestCases(BlockFlow blockFlow)
		{
			var excludedChars = blockFlow switch
			{
				BlockFlow.BlockKey => Enumerable.Empty<string>(),
				BlockFlow.FlowKey => CharStore.FlowIndicators,
				_ => throw new ArgumentOutOfRangeException(
					nameof(blockFlow),
					blockFlow,
					$"Only {BlockFlow.BlockKey} and {BlockFlow.FlowKey} can be processed."
				)
			};

			var nsPlainSafeCharsWithoutSurrogates =
				CharStore.GetNsCharsWithoutSurrogates().Except(excludedChars).ToList();

			var nsPlainSafeSurrogates = CharStore.SurrogatePairs.Value;

			foreach (var nsPlainSafeChars in new[] { nsPlainSafeCharsWithoutSurrogates, nsPlainSafeSurrogates })
				foreach (var nbNsPlainInLine in createNbNsPlainInLineFrom(nsPlainSafeChars))
					yield return nbNsPlainInLine;
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

			const int groupItemCount = Characters.CharGroupLength;
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

		private static IEnumerable<string> getNegativeTextCases(BlockFlow blockFlow)
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

			IReadOnlyCollection<string> invalidNsPlainSafes = blockFlow switch
			{
				BlockFlow.BlockKey => new[] { " " },
				BlockFlow.FlowKey => CharStore.FlowIndicators.ToList(),
				_ => throw new ArgumentOutOfRangeException(
					nameof(blockFlow),
					blockFlow,
					$"Only {BlockFlow.BlockKey} and {BlockFlow.FlowKey} can be processed."
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
			var tooManyNsPlainChars = Helpers.RepeatAndJoin(nsPlainChar, Characters.CharGroupLength + 1);
			yield return nsPlainFirst + tooManyNsPlainChars;

			// Too long ns plain in line
			var tooLongNsPlainInLine =
				Helpers.RepeatAndJoin(whiteChar + nsPlainChar, Characters.CharGroupLength) + whiteChar;
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
	}
}