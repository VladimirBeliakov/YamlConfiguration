using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using YamlConfiguration.Processor.FlowStyles;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class DoubleQuotedMultiLineTests : QuotedMultiLineBaseTest
	{
		[TestCaseSource(nameof(getFirstLinePositiveTestCases))]
		public void ValidDoubleQuotedFirstLine_ReturnsCorrectLineTypeAndExtractedValue(
			MultiLineOneLineTestCase testCase
		)
		{
			var multiLine = new DoubleQuotedStyle.MultiLine();

			var firstLineResult = multiLine.ProcessFirstLine(testCase.TestValue);

			Assert.That(firstLineResult.LineType, Is.EqualTo(testCase.Result.LineType));
			Assert.That(firstLineResult.ExtractedValue, Is.EqualTo(testCase.Result.ExtractedValue));
		}

		[TestCaseSource(nameof(getNextLinePositiveTestCases))]
		public void ValidDoubleQuotedNextLine_ReturnsCorrectLineTypeAndExtractedValue(MultiLineOneLineTestCase testCase)
		{
			var multiLine = new DoubleQuotedStyle.MultiLine();
			multiLine.ProcessFirstLine("\"");

			var nextLineResult = multiLine.ProcessNextLine(testCase.TestValue);

			Assert.That(nextLineResult.LineType, Is.EqualTo(testCase.Result.LineType));
			Assert.That(nextLineResult.ExtractedValue, Is.EqualTo(testCase.Result.ExtractedValue));
		}

		[TestCaseSource(nameof(GetFirstLineNegativeTestCases), new Object[] { true })]
		public void InvalidDoubleQuotedFirstLine_ReturnsInvalid(string testCase)
		{
			var multiLine = new DoubleQuotedStyle.MultiLine();

			var firstLineResult = multiLine.ProcessFirstLine(testCase);

			Assert.That(firstLineResult.LineType, Is.EqualTo(LineType.Invalid));
		}

		[TestCaseSource(nameof(GetNextLineNegativeTestCases), new Object[] { true })]
		public void InvalidDoubleQuotedNextLine_ReturnsInvalid(string testCase)
		{
			var multiLine = new DoubleQuotedStyle.MultiLine();
			multiLine.ProcessFirstLine("\"");

			var nextLineResult = multiLine.ProcessNextLine(testCase);

			Assert.That(nextLineResult.LineType, Is.EqualTo(LineType.Invalid));
		}

		[Test]
		public void ProcessDoubleQuotedFirstLine_CalledTwice_Throws()
		{
			var multiLine = new DoubleQuotedStyle.MultiLine();
			multiLine.ProcessFirstLine("\"");

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessFirstLine("\""));
		}

		[Test]
		public void ProcessDoubleQuotedNextLine_FirstLineWasNotProcessed_Throws()
		{
			var multiLine = new DoubleQuotedStyle.MultiLine();

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessNextLine(string.Empty));
		}

		[Test]
		public void ProcessDoubleQuotedNextLine_LastLineAlreadyProcessed_Throws()
		{
			var multiLine = new DoubleQuotedStyle.MultiLine();
			multiLine.ProcessFirstLine("\"");
			multiLine.ProcessNextLine("\"");

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessNextLine(string.Empty));
		}

		private static IEnumerable<MultiLineOneLineTestCase> getFirstLinePositiveTestCases() =>
			GetFirstLines(isDoubleQuoted: true, withEscapedBreak: false)
				.Concat(GetFirstLines(isDoubleQuoted: true, withEscapedBreak: true));

		private static IEnumerable<MultiLineOneLineTestCase> getNextLinePositiveTestCases() =>
			GetEmptyNextLines(isDoubleQuoted: true, isLastLine: false)
				.Concat(GetEmptyNextLines(isDoubleQuoted: true, isLastLine: true))
				.Concat(GetNonEmptyLines(isDoubleQuoted: true, withEscapedBreak: false, isLastLine: false))
				.Concat(GetNonEmptyLines(isDoubleQuoted: true, withEscapedBreak: true, isLastLine: false))
				.Concat(GetNonEmptyLines(isDoubleQuoted: true, withEscapedBreak: false, isLastLine: true));

	}
}