using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Processor;
using Processor.FlowStyles;

namespace ProcessorTests
{
	[TestFixture]
	public class OneLineTests
	{
		[TestCaseSource(nameof(getPositiveOneLineTestCases))]
		public void ValidOneLine_ReturnsTrueAndExtractedValue(RegexTestCase testCase)
		{
			var isSuccess = DoubleQuotedStyles.TryProcessOneLine(testCase.TestValue, out var extractedValue);

			Assert.True(isSuccess);
			Assert.That(extractedValue, Is.EqualTo(testCase.WholeMatch));
		}

		[TestCaseSource(nameof(getNegativeOneLineCases))]
		public void InvalidOneLine_ReturnsFalseAndNullAsExtractedValue(string testCase)
		{
			var isSuccess = DoubleQuotedStyles.TryProcessOneLine(testCase, out var extractedValue);

			Assert.False(isSuccess);
			Assert.Null(extractedValue);
		}

		private static IEnumerable<RegexTestCase> getPositiveOneLineTestCases()
		{
			var chars = CharStore.Chars;

			var nbDoubleOneLines =
				CharStore.NbDoubleCharsWithoutEscapedAndSurrogates.Value.GroupBy(Characters.CharGroupLength)
					.Concat(CharStore.SurrogatePairs.Value.GroupBy(Characters.CharGroupLength))
					.Concat(CharStore.EscapedChars.GroupBy(Characters.CharGroupLength))
					.Append(string.Empty);

			foreach (var nbDoubleOneLine in nbDoubleOneLines)
			{
				yield return new RegexTestCase(
					chars + "\"" + nbDoubleOneLine + "\"" + chars,
					nbDoubleOneLine
				);
			}
		}

		private static IEnumerable<string> getNegativeOneLineCases()
		{
			var chars = CharStore.Chars;
			var tooManyNbDoubleChars = CharStore.GetCharRange("a") + "a";

			yield return chars + "\"" + "\\" + "\"" + chars;
			yield return chars + "\"" + "a" + chars;
			yield return chars + "a" + "\"" + chars;
			yield return chars + "a" + chars;
			yield return chars + "\"" + tooManyNbDoubleChars + "\"" + chars;
		}
	}
}
