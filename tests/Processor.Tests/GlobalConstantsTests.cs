using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Processor;

namespace Processor.Tests
{
	// TODO: Add more characters to the test cases.
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class GlobalConstantsTests
	{
		[TestCaseSource(nameof(getC0AndC1BlockExcludedChars))]
		public void ForbiddenChars_ExcludedSymbolsDontMatch(string excludedChar)
		{
			var testString = excludedChar;
			Assert.False(_forbiddenCharsRegex.IsMatch(testString));
		}

		[TestCaseSource(nameof(getC0AndC1BlockExcludedChars))]
		public void PrintableChars_ExcludedSymbolsMatch(string excludedChar)
		{
			var testString = excludedChar;
			Assert.True(_printableCharsRegex.IsMatch(testString));
		}

		[TestCaseSource(nameof(getBreakMatchableCases))]
		public void BreakRegex_MatchesNewLine(string value, int matchCount)
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

		private static readonly Regex _forbiddenCharsRegex =
			new Regex(Characters.ForbiddenCharsRegex, RegexOptions.Compiled);
		
		private static readonly Regex _printableCharsRegex =
			new Regex(Characters.PrintableChar, RegexOptions.Compiled);

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

		private static IEnumerable<TestCaseData> getBreakMatchableCases()
		{
			var newLine = Environment.NewLine;
			yield return new TestCaseData($"ABC{newLine}ABC", 1);
			yield return new TestCaseData($"ABC{newLine}{newLine}ABC", 2);
			yield return new TestCaseData(@"ABC 
											ABC", 1);
		}

		private const string TAB = "\u0009";
		private const string LF = "\u000A";
		private const string CR = "\u000D";
		private const string NEL = "\u0085";

		private readonly Regex _breakRegex = new Regex(BasicStructures.Break, RegexOptions.Compiled);
	}
}
