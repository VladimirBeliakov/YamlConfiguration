using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class EscapedCharsTests
	{
		[TestCaseSource(nameof(getEscapedCharTestCases))]
		public void EscapedChar_ValidEscapedChars_Matches(RegexTestCase testCase)
		{
			var regex = _escapedCharRegex.Match(testCase.TestValue);

			Assert.That(regex.Value, Is.EqualTo(testCase.WholeMatch));
		}

		private static IEnumerable<RegexTestCase> getEscapedCharTestCases()
		{
			var escapedChars = CharStore.EscapedChars;

			var chars = CharStore.Chars;

			foreach (var escapedChar in escapedChars)
			{
				yield return new RegexTestCase(
					testValue: chars + escapedChar + chars,
					wholeMatch: escapedChar
				);
			}
		}

		private static readonly Regex _escapedCharRegex = new Regex(Characters.EscapedChar, RegexOptions.Compiled);
	}
}