using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class PlainMultilineParserTests
	{
		[TestCaseSource(nameof(getNotFlowContexts))]
		public void TryProcess_NotFlowContext_Throws(Context context)
		{
			var stream = A.Fake<ICharacterStream>();

			Assert.ThrowsAsync<ArgumentException>(() => createParser().TryProcess(stream, 0, context).AsTask());
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getFlowContexts))]
		public async Task TryProcess_PlainInOneLineParserReturnsNull_ReturnsNull(
			Context context
		)
		{
			var stream = A.Fake<ICharacterStream>();
			var expectedPlainLineNode = new PlainLineNode(Guid.NewGuid().ToString());
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context)).Returns(null);

			var result = await createParser(plainInOneLineParser).TryProcess(stream, indentLength: 0, context);

			Assert.Null(result);
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getFlowContexts))]
		public async Task TryProcess_NoNextLine_ReturnsValueOnlyFromInOneLineNode(Context context)
		{
			var stream = A.Fake<ICharacterStream>();
			var plainLineNode = new PlainLineNode(Guid.NewGuid().ToString());
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context)).Returns(null);

			var result = await createParser(plainInOneLineParser, plainNextLineParser)
				.TryProcess(stream, indentLength: 0, context);

			Assert.That(result?.Value, Is.EqualTo(plainLineNode.Value));
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getFlowContexts))]
		public void TryProcess_NoFoldedLinesBetweenPlainLines_Throws(Context context)
		{
			var stream = A.Fake<ICharacterStream>();
			var plainLineNode = new PlainLineNode(Guid.NewGuid().ToString());
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context)).Returns(Guid.NewGuid().ToString());
			var flowFoldedLinesParser = A.Fake<IFlowFoldedLinesParser>();
			A.CallTo(() => flowFoldedLinesParser.Process(stream, A<uint>._)).Returns(
				new FlowFoldedLinesResult(separateInLineWhiteSpaceCount: 0, foldedLineResult: null)
			);

			var parser = createParser(plainInOneLineParser, plainNextLineParser, flowFoldedLinesParser);
			Assert.ThrowsAsync<InvalidYamlException>(() =>
				parser.TryProcess(stream, indentLength: 0, context).AsTask()
			);
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getFlowContexts))]
		public async Task TryProcess_BreakAsSpaceBetweenPlainLines_ReturnsLinesFoldedBySpace(Context context)
		{
			var stream = A.Fake<ICharacterStream>();
			const uint indentLength = 1;
			var firstLine = Guid.NewGuid().ToString();
			var nextLine = Guid.NewGuid().ToString();
			var plainLineNode = new PlainLineNode(firstLine);
			var flowFoldedLinesResult =
				new FlowFoldedLinesResult(
					separateInLineWhiteSpaceCount: 0,
					foldedLineResult: new FoldedLinesResult(emptyLineCount: 0, isBreakAsSpace: true)
				);
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context)).Returns(nextLine).Once();
			var flowFoldedLinesParser = A.Fake<IFlowFoldedLinesParser>();
			A.CallTo(() => flowFoldedLinesParser.Process(stream, indentLength)).Returns(flowFoldedLinesResult);

			var parser = createParser(plainInOneLineParser, plainNextLineParser, flowFoldedLinesParser);
			var result = await parser.TryProcess(stream, indentLength, context);

			Assert.That(result?.Value, Is.EqualTo($"{firstLine} {nextLine}"));
			stream.AssertNotAdvanced();
		}

		[Test]
		public async Task TryProcess_EmptyLinesBetweenPlainLines_ReturnsLinesFoldedByBreak(
			[ValueSource(nameof(getFlowContexts))] Context context,
			[Values(1U, 5U, 10U)] uint emptyLineCount
		)
		{
			var stream = A.Fake<ICharacterStream>();
			const uint indentLength = 1;
			var firstLine = Guid.NewGuid().ToString();
			var nextLine = Guid.NewGuid().ToString();
			var plainLineNode = new PlainLineNode(firstLine);
			var flowFoldedLinesResult =
				new FlowFoldedLinesResult(
					separateInLineWhiteSpaceCount: 0,
					foldedLineResult: new FoldedLinesResult(emptyLineCount)
				);
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context)).Returns(nextLine).Once();
			var flowFoldedLinesParser = A.Fake<IFlowFoldedLinesParser>();
			A.CallTo(() => flowFoldedLinesParser.Process(stream, indentLength)).Returns(flowFoldedLinesResult);

			var parser = createParser(plainInOneLineParser, plainNextLineParser, flowFoldedLinesParser);
			var result = await parser.TryProcess(stream, indentLength, context);

			var breaks = CharStore.Repeat('\n', (int) emptyLineCount);
			Assert.That(result?.Value, Is.EqualTo($"{firstLine}{breaks}{nextLine}"));
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getFlowContexts))]
		public async Task
			TryProcess_BreakAsSpaceAndEmptyLinesBetweenPlainLines_ReturnsLinesFoldedBySpaceAndBreak(
				Context context
			)
		{
			var stream = A.Fake<ICharacterStream>();
			const uint indentLength = 1;
			const uint firstEmptyLineCount = 1;
			const uint secondEmptyLineCount = 2;
			var firstLine = Guid.NewGuid().ToString();
			var secondLine = Guid.NewGuid().ToString();
			var thirdLine = Guid.NewGuid().ToString();
			var forthLine = Guid.NewGuid().ToString();
			var plainLineNode = new PlainLineNode(firstLine);
			var firstFlowFoldedLinesResult =
				new FlowFoldedLinesResult(
					separateInLineWhiteSpaceCount: 0,
					foldedLineResult: new FoldedLinesResult(firstEmptyLineCount)
				);
			var secondFlowFoldedLinesResult =
				new FlowFoldedLinesResult(
					separateInLineWhiteSpaceCount: 0,
					foldedLineResult: new FoldedLinesResult(emptyLineCount: 0, isBreakAsSpace: true)
				);
			var thirdFlowFoldedLinesResult =
				new FlowFoldedLinesResult(
					separateInLineWhiteSpaceCount: 0,
					foldedLineResult: new FoldedLinesResult(secondEmptyLineCount)
				);
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context))
				.Returns(secondLine).Once().Then
				.Returns(thirdLine).Once().Then
				.Returns(forthLine).Once();
			var flowFoldedLinesParser = A.Fake<IFlowFoldedLinesParser>();
			A.CallTo(() => flowFoldedLinesParser.Process(stream, indentLength))
				.Returns(firstFlowFoldedLinesResult).Once().Then
				.Returns(secondFlowFoldedLinesResult).Once().Then
				.Returns(thirdFlowFoldedLinesResult).Once();

			var parser = createParser(plainInOneLineParser, plainNextLineParser, flowFoldedLinesParser);
			var result = await parser.TryProcess(stream, indentLength, context);

			var firstBreaks = CharStore.Repeat('\n', (int) firstEmptyLineCount);
			var secondBreaks = CharStore.Repeat('\n', (int) secondEmptyLineCount);
			var expectedResult = $"{firstLine}{firstBreaks}{secondLine} {thirdLine}{secondBreaks}{forthLine}";
			Assert.That(result?.Value, Is.EqualTo(expectedResult));
			stream.AssertNotAdvanced();
		}

		private static PlainMultilineParser createParser(
			IPlainInOneLineParser? plainInOneLineParser = null,
			IPlainNextLineParser? plainNextLineParser = null,
			IFlowFoldedLinesParser? flowFoldedLinesParser = null
		)
		{
			var defaultPlainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() =>
				defaultPlainInOneLineParser.TryProcess(A<ICharacterStream>._, A<Context>._)
			).Returns(new PlainLineNode(String.Empty));

			return new(
				plainInOneLineParser ?? defaultPlainInOneLineParser,
				plainNextLineParser ?? A.Fake<IPlainNextLineParser>(),
				flowFoldedLinesParser ?? A.Fake<IFlowFoldedLinesParser>()
			);
		}

		private static IEnumerable<Context> getNotFlowContexts() =>
			Enum.GetValues<Context>().Except(getFlowContexts());

		private static IEnumerable<Context> getFlowContexts()
		{
			yield return Context.FlowIn;
			yield return Context.FlowOut;
		}
	}
}
