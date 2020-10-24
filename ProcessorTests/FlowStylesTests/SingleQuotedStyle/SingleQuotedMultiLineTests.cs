using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Processor;
using Processor.FlowStyles;

namespace ProcessorTests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class SingleQuotedMultiLineTests : QuotedMultiLineBaseTest
	{
		[TestCaseSource(nameof(getFirstLinePositiveTestCases))]
		public void ValidSingleQuotedFirstLine_ReturnsCorrectLineTypeAndExtractedValue(
			MultiLineOneLineTestCase testCase
		)
		{
			var multiLine = new SingleQuotedStyle.MultiLine();

			var firstLineResult = multiLine.ProcessFirstLine(testCase.TestValue);

			Assert.That(firstLineResult.LineType, Is.EqualTo(testCase.Result.LineType));
			Assert.That(firstLineResult.ExtractedValue, Is.EqualTo(testCase.Result.ExtractedValue));
		}

		[TestCaseSource(nameof(getNextLinePositiveTestCases))]
		public void ValidSingleQuotedNextLine_ReturnsCorrectLineTypeAndExtractedValue(MultiLineOneLineTestCase testCase)
		{
			var multiLine = new SingleQuotedStyle.MultiLine();
			multiLine.ProcessFirstLine("\'");

			var nextLineResult = multiLine.ProcessNextLine(testCase.TestValue);

			Assert.That(nextLineResult.LineType, Is.EqualTo(testCase.Result.LineType));
			Assert.That(nextLineResult.ExtractedValue, Is.EqualTo(testCase.Result.ExtractedValue));
		}

		[TestCaseSource(nameof(GetFirstLineNegativeTestCases), new Object[] { false })]
		public void InvalidSingleQuotedFirstLine_ReturnsInvalid(string testCase)
		{
			var multiLine = new SingleQuotedStyle.MultiLine();

			var firstLineResult = multiLine.ProcessFirstLine(testCase);

			Assert.That(firstLineResult.LineType, Is.EqualTo(LineType.Invalid));
		}

		[TestCaseSource(nameof(GetNextLineNegativeTestCases), new Object[] { false })]
		public void InvalidSingleQuotedNextLine_ReturnsInvalid(string testCase)
		{
			var multiLine = new SingleQuotedStyle.MultiLine();
			multiLine.ProcessFirstLine("\'");

			var nextLineResult = multiLine.ProcessNextLine(testCase);

			Assert.That(nextLineResult.LineType, Is.EqualTo(LineType.Invalid));
		}

		[Test]
		public void ProcessSingleQuotedFirstLine_CalledTwice_Throws()
		{
			var multiLine = new SingleQuotedStyle.MultiLine();
			multiLine.ProcessFirstLine("\'");

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessFirstLine("\'"));
		}

		[Test]
		public void ProcessSingleQuotedNextLine_FirstLineWasNotProcessed_Throws()
		{
			var multiLine = new SingleQuotedStyle.MultiLine();

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessNextLine(string.Empty));
		}

		[Test]
		public void ProcessSingleQuotedNextLine_LastLineAlreadyProcessed_Throws()
		{
			var multiLine = new SingleQuotedStyle.MultiLine();
			multiLine.ProcessFirstLine("\'");
			multiLine.ProcessNextLine("\'");

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessNextLine(string.Empty));
		}

		private static IEnumerable<MultiLineOneLineTestCase> getFirstLinePositiveTestCases() =>
			GetFirstLines(isDoubleQuoted: false);

		private static IEnumerable<MultiLineOneLineTestCase> getNextLinePositiveTestCases() =>
			GetEmptyNextLines(isDoubleQuoted: false, isLastLine: false)
				.Concat(GetEmptyNextLines(isDoubleQuoted: false, isLastLine: true))
				.Concat(GetNonEmptyLines(isDoubleQuoted: false, isLastLine: false))
				.Concat(GetNonEmptyLines(isDoubleQuoted: false, isLastLine: true));
	}
}
