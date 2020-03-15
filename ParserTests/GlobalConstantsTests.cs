using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Parser;
using Parser.TypeDefinitions;

namespace DeserializerTests
{
	// TODO: Add more characters to the test cases.
	[TestFixture, Parallelizable(ParallelScope.Children)]
	public class GlobalConstantsTests
	{
		[TestCaseSource(nameof(getC0AndC1BlockExcludedChars))]
		public void ForbiddenChars_ExcludedSymbolsDontMatch(string excludedChar)
		{
			var testString = excludedChar;
			Assert.False(forbiddenCharsRegex.IsMatch(testString));
		}

		[TestCaseSource(nameof(getC0AndC1BlockExcludedChars))]
		public void PrintableChars_ExcludedSymbolsMatch(string excludedChar)
		{
			var testString = excludedChar;
			Assert.True(printableCharsRegex.IsMatch(testString));
		}

		[TestCase("ABC\rABC", 1)]
		[TestCase("ABC\nABC", 1)]
		[TestCase("ABC\r\nABC", 1)]
		[TestCase("ABC\r\rABC", 2)]
		[TestCase("ABC\n\nABC", 2)]
		[TestCase("ABC\r\n\r\nABC", 2)]
		[TestCase(@"ABC
					ABC", 1)]
		public void BreakRegex_MatchesCrAndLf(string value, int matchCount)
		{
			var matches = _breakRegex.Matches(value);

			Assert.That(matches.Count, Is.EqualTo(matchCount));
		}

		[TestCase("ABCABC")]
		public void BreakRegex_DoesNotMatchOtherChars(string otherChars)
		{
			var matches = _breakRegex.Matches(otherChars);

			Assert.That(matches.Count, Is.EqualTo(0));
		}
		
		[TestCase("Space Space", 1)]
		[TestCase("Tab\tTab", 1)]
		[TestCase("Space \tTab", 1)]
		[TestCase("Tab\t Space", 1)]
		[TestCaseSource(nameof(getSeparateInLineLongTestStrings))]
		public void SeparateInLine_MatchesWhites(string value, int matchCount)
		{
			var matches = _separateInLineRegex.Matches(value);

			Assert.That(matches.Count, Is.EqualTo(matchCount));
		}

		[TestCase("ABCABC")]
		public void SeparateInLine_DoesNotMatchOtherChars(string otherChars)
		{
			var matches = _separateInLineRegex.Matches(otherChars);

			Assert.That(matches.Count, Is.EqualTo(0));
		}

		[TestCaseSource(nameof(getSeparateInLineTooLongTestStrings))]
		public void SeparateInLine_TooLongString_MatchesTwice(string tooLongString, int matchCount)
		{
			var matches = _separateInLineRegex.Matches(tooLongString);

			Assert.That(matches.Count, Is.EqualTo(matchCount));
		}

		[TestCaseSource(nameof(getBlockFlowWithCorrespondingRegex))]
		public void LinePrefix_ReturnsCorrespondingRegexForBlockFlow(BlockFlowInOut value, string expectedRegex)
		{
			var actualRegex = GlobalConstants.LinePrefix(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		// TODO: Add more cases.
		[TestCase(BlockFlowInOut.BlockIn, " ABC")]
		[TestCase(BlockFlowInOut.BlockOut," \tABC")]
		public void LinePrefix_Block_MatchesOnlySpaces(BlockFlowInOut type, string value)
		{
			var regex = new Regex(GlobalConstants.LinePrefix(type));

			var matches = regex.Matches(value);

			Assert.That(matches.Count, Is.EqualTo(1));
		}

		private static readonly Regex forbiddenCharsRegex =
			new Regex(GlobalConstants.ForbiddenCharsRegex, RegexOptions.Compiled);
		
		private static readonly Regex printableCharsRegex =
			new Regex(GlobalConstants.PrintableCharsRegex, RegexOptions.Compiled);

		private static IEnumerable<string> getC0AndC1BlockExcludedChars()
		{
			foreach (var excludedChar in getC0ControlBlockExcludedChars())
				yield return excludedChar;

			foreach (var excludedChar in getC1ControlBlockExcludedChars())
				yield return excludedChar;
		}

		private static IEnumerable<string> getC0ControlBlockExcludedChars()
		{
			yield return TAB;
			yield return LF;
			yield return CR;
		}

		private static IEnumerable<string> getC1ControlBlockExcludedChars()
		{
			yield return NEL;
		}

		private const string TAB = "\u0009";
		private const string LF = "\u000A";
		private const string CR = "\u000D";
		private const string NEL = "\u0085";

		private static IEnumerable<TestCaseData> getSeparateInLineLongTestStrings()
		{
			yield return new TestCaseData(new String(Enumerable.Repeat(' ', 100).ToArray()), 1);
			yield return new TestCaseData(new String(Enumerable.Repeat('\t', 100).ToArray()), 1);
			yield return new TestCaseData(
				new String(Enumerable.Repeat('\t', 50).Concat(Enumerable.Repeat(' ', 50)).ToArray()), 1);
		}

		private static IEnumerable<TestCaseData> getSeparateInLineTooLongTestStrings()
		{
			yield return new TestCaseData(new String(Enumerable.Repeat(' ', 101).ToArray()), 2);
			yield return new TestCaseData(new String(Enumerable.Repeat('\t', 101).ToArray()), 2);
			yield return new TestCaseData(
				new String(Enumerable.Repeat(' ', 51).Concat(Enumerable.Repeat('\t', 50)).ToArray()), 2);
			yield return new TestCaseData(
				new String(Enumerable.Repeat('\t', 51).Concat(Enumerable.Repeat(' ', 50)).ToArray()), 2);
		}

		private static IEnumerable<TestCaseData> getBlockFlowWithCorrespondingRegex()
		{
			yield return new TestCaseData(BlockFlowInOut.BlockIn, " {1,100}");
			yield return new TestCaseData(BlockFlowInOut.BlockOut, " {1,100}");
			yield return new TestCaseData(BlockFlowInOut.FlowIn, " {1,100}([ \t]{1,100})?");
			yield return new TestCaseData(BlockFlowInOut.FlowOut, " {1,100}([ \t]{1,100})?");
		}

		private readonly Regex _breakRegex = new Regex(GlobalConstants.Break, RegexOptions.Compiled);
		private readonly Regex _separateInLineRegex = new Regex(GlobalConstants.SeparateInLine, RegexOptions.Compiled);
	}
}
