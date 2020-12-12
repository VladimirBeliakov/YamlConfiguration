using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Processor;
using Processor.FlowStyles;

namespace ProcessorTests
{
	[TestFixture]
	public class DoubleQuotedOneLineTests
	{
		[TestCaseSource(nameof(getPositiveOneLineTestCases))]
		public void ValidOneDoubleQuotedLine_ReturnsTrueAndExtractedValue(Tuple<string, string> testCase)
		{
			var (testValue, expectedExtractedValue) = testCase;

			var isSuccess = DoubleQuotedStyle.TryProcessOneLine(testValue, out var extractedValue);

			Assert.True(isSuccess);
			Assert.That(extractedValue, Is.EqualTo(expectedExtractedValue));
		}

		[TestCaseSource(nameof(getNegativeOneLineCases))]
		public void InvalidOneDoubleQuotedLine_ReturnsFalseAndNullAsExtractedValue(string testCase)
		{
			var isSuccess = DoubleQuotedStyle.TryProcessOneLine(testCase, out var extractedValue);

			Assert.False(isSuccess);
			Assert.Null(extractedValue);
		}

		private static IEnumerable<Tuple<string, string>> getPositiveOneLineTestCases()
		{
			var chars = CharStore.Chars;

			var nbDoubleOneLines =
				CharStore.NbNsDoubleCharsWithoutEscapedAndSurrogates.Value.GroupBy(Characters.CharGroupLength)
					.Concat(CharStore.SurrogatePairs.Value.GroupBy(Characters.CharGroupLength))
					.Concat(CharStore.EscapedChars.GroupBy(Characters.CharGroupLength))
					.Append(string.Empty);

			foreach (var nbDoubleOneLine in nbDoubleOneLines)
			{
				yield return Tuple.Create(
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
