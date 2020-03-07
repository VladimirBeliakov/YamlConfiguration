using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Parser;

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
	}
}