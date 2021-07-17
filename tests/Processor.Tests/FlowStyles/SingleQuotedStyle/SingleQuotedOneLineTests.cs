using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using YamlConfiguration.Processor.FlowStyles;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture]
	public class SingleQuotedOneLineTests
	{
		[TestCaseSource(nameof(getPositiveOneLineTestCases))]
		public void ValidOneSingleQuotedLine_ReturnsTrueAndExtractedValue(Tuple<string, string> testCase)
		{
			var (testValue, expectedExtractedValue) = testCase;

			var isSuccess = SingleQuotedStyle.TryProcessOneLine(testValue, out var extractedValue);

			Assert.True(isSuccess);
			Assert.That(extractedValue, Is.EqualTo(expectedExtractedValue));
		}

		[TestCaseSource(nameof(getNegativeOneLineCases))]
		public void InvalidOneSingleQuotedLine_ReturnsFalseAndNullAsExtractedValue(string testCase)
		{
			var isSuccess = SingleQuotedStyle.TryProcessOneLine(testCase, out var extractedValue);

			Assert.False(isSuccess);
			Assert.Null(extractedValue);
		}

		private static IEnumerable<Tuple<string, string>> getPositiveOneLineTestCases()
		{
			var chars = CharStore.Chars;

			var nbSingleOneLines =
				CharStore.NbNsSingleCharsWithoutSurrogates.Value
					.GroupBy(Characters.CharGroupMaxLength)
					.Concat(CharStore.SurrogatePairs.Value.GroupBy(Characters.CharGroupMaxLength))
					.Append(CharStore.GetCharRange("''"))
					.Append(string.Empty);

			foreach (var nbSingleOneLine in nbSingleOneLines)
			{
				yield return Tuple.Create(
					chars + "'" + nbSingleOneLine + "'" + chars,
					nbSingleOneLine
				);
			}
		}

		private static IEnumerable<string> getNegativeOneLineCases()
		{
			var chars = CharStore.Chars;
			var tooManyNbSingleChars = CharStore.GetCharRange("a") + "a";

			yield return chars + "'" + "\u0019" + "'" + chars;
			yield return chars + "'" + "a" + chars;
			yield return chars + "a" + "'" + chars;
			yield return chars + "a" + chars;
			yield return chars + "'" + tooManyNbSingleChars + "'" + chars;
		}
	}
}
