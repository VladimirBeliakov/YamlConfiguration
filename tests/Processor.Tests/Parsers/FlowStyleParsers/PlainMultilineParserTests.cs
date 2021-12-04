using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
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
		public async Task TryProcess_NotAllCharsInFirstLineArePlainInOneLine_ReturnsOnlyInOneLineNode(
			Context context
		)
		{
			var stream = A.Fake<ICharacterStream>();
			var expectedPlainLineNode = new PlainLineNode(Guid.NewGuid().ToString());
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context, true)).Returns(null);
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context, false)).Returns(expectedPlainLineNode);

			var actualPlainLineNode = await createParser(plainInOneLineParser)
				.TryProcess(stream, indentLength: 0, context);

			Assert.That(actualPlainLineNode, Is.SameAs(expectedPlainLineNode));
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getFlowContexts))]
		public async Task TryProcess_NoNextLine_ReturnsValueOnlyFromInOneLineNode(Context context)
		{
			var stream = A.Fake<ICharacterStream>();
			var plainLineNode = new PlainLineNode(Guid.NewGuid().ToString());
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context, true)).Returns(plainLineNode);
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
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context, true)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context)).Returns(Guid.NewGuid().ToString());
			var flowFoldedLinesParser = A.Fake<IFlowFoldedLinesParser>();
			A.CallTo(() => flowFoldedLinesParser.TryProcess(stream, A<uint>._)).Returns(null);

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
			var foldedLinesResult = new FoldedLinesResult(emptyLineCount: 0, isBreakAsSpace: true);
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context, true)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context)).Returns(nextLine).Once();
			var flowFoldedLinesParser = A.Fake<IFlowFoldedLinesParser>();
			A.CallTo(() => flowFoldedLinesParser.TryProcess(stream, indentLength)).Returns(foldedLinesResult);

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
			var foldedLinesResult = new FoldedLinesResult(emptyLineCount);
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context, true)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context)).Returns(nextLine).Once();
			var flowFoldedLinesParser = A.Fake<IFlowFoldedLinesParser>();
			A.CallTo(() => flowFoldedLinesParser.TryProcess(stream, indentLength)).Returns(foldedLinesResult);

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
			var firstFoldedLinesResult = new FoldedLinesResult(firstEmptyLineCount);
			var secondFoldedLinesResult = new FoldedLinesResult(emptyLineCount: 0, isBreakAsSpace: true);
			var thirdFoldedLinesResult = new FoldedLinesResult(secondEmptyLineCount);
			var plainInOneLineParser = A.Fake<IPlainInOneLineParser>();
			A.CallTo(() => plainInOneLineParser.TryProcess(stream, context, true)).Returns(plainLineNode);
			var plainNextLineParser = A.Fake<IPlainNextLineParser>();
			A.CallTo(() => plainNextLineParser.TryProcess(stream, context))
				.Returns(secondLine).Once().Then
				.Returns(thirdLine).Once().Then
				.Returns(forthLine).Once();
			var flowFoldedLinesParser = A.Fake<IFlowFoldedLinesParser>();
			A.CallTo(() => flowFoldedLinesParser.TryProcess(stream, indentLength))
				.Returns(firstFoldedLinesResult).Once().Then
				.Returns(secondFoldedLinesResult).Once().Then
				.Returns(thirdFoldedLinesResult).Once();

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
				defaultPlainInOneLineParser.TryProcess(A<ICharacterStream>._, A<Context>._, A<bool>._)
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
