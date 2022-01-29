using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class FlowFoldedLinesParserTests
	{
		[Test]
		public async Task TryProcess_StreamWithoutFoldedLinesAndSeparateInLine_ReturnsEmptyResult()
		{
			var foldedLineParser = A.Fake<IFoldedLinesParser>();
			A.CallTo(() => foldedLineParser.Process(A<ICharacterStream>._)).Returns(null);
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			var separateInLineResult = new ParsedSeparateInLineResult(isSeparateInLine: false, whiteSpaceCount: 0);
			A.CallTo(() => separateInLineParser.Peek(A<ICharacterStream>._)).Returns(separateInLineResult);

			var charStream = A.Fake<ICharacterStream>();
			var result =
				await createParser(foldedLineParser).Process(charStream, indentLength: 0);

			Assert.Null(result.FoldedLineResult);
			Assert.Zero(result.SeparateInLineWhiteSpaceCount);
			A.CallTo(() => charStream.Read(A<uint>._)).MustNotHaveHappened();
		}

		[Test]
		public void TryProcess_StreamWithFoldedLinesButNoFlowLinePrefix_Throws()
		{
			var foldedLineParser = A.Fake<IFoldedLinesParser>();
			A.CallTo(() => foldedLineParser.Process(A<ICharacterStream>._))
				.Returns(new FoldedLinesResult(emptyLineCount: 1));
			var flowLinePrefixParser = A.Fake<IFlowLinePrefixParser>();
			A.CallTo(() => flowLinePrefixParser.TryProcess(A<ICharacterStream>._, A<uint>._)).Returns(false);

			var parser = createParser(foldedLineParser, flowLinePrefixParser);

			Assert.ThrowsAsync<InvalidYamlException>(
				() => ((IFlowFoldedLinesParser) parser).Process(A.Dummy<ICharacterStream>(), indentLength: 0).AsTask()
			);
		}

		[Test]
		public async Task TryProcess_StreamWithFoldedLinesAndFlowLinePrefix_ReturnsFlowFoldedLinesResult()
		{
			var foldedLinesResult = new FoldedLinesResult(emptyLineCount: 1);
			var foldedLineParser = A.Fake<IFoldedLinesParser>();
			A.CallTo(() => foldedLineParser.Process(A<ICharacterStream>._)).Returns(foldedLinesResult);
			var flowLinePrefixParser = A.Fake<IFlowLinePrefixParser>();
			A.CallTo(() => flowLinePrefixParser.TryProcess(A<ICharacterStream>._, A<uint>._)).Returns(true);
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			var separateInLineResult = new ParsedSeparateInLineResult(isSeparateInLine: true, whiteSpaceCount: 2);
			A.CallTo(() => separateInLineParser.Peek(A<ICharacterStream>._)).Returns(separateInLineResult);

			var parser = createParser(foldedLineParser, flowLinePrefixParser, separateInLineParser);
			var charStream = A.Fake<ICharacterStream>();
			var result = await ((IFlowFoldedLinesParser) parser).Process(charStream, indentLength: 0);

			Assert.That(result.FoldedLineResult, Is.SameAs(foldedLinesResult));
			Assert.That(result.SeparateInLineWhiteSpaceCount, Is.EqualTo(separateInLineResult.WhiteSpaceCount));
			A.CallTo(() => charStream.Read(separateInLineResult.WhiteSpaceCount)).MustHaveHappenedOnceExactly();
		}

		private static FlowFoldedLinesParser createParser(
			IFoldedLinesParser? foldedLinesParser = null,
			IFlowLinePrefixParser? flowLinePrefixParser = null,
			ISeparateInLineParser? separateInLineParser = null
		)
		{
			var defaultSeparateInLineParser = A.Dummy<ISeparateInLineParser>();
			var defaultFoldedLineParser = A.Fake<IFoldedLinesParser>();

			A.CallTo(() => defaultFoldedLineParser.Process(A<ICharacterStream>._))
				.Returns(new FoldedLinesResult(emptyLineCount: 1));

			var defaultFlowLinePrefixParser = A.Fake<IFlowLinePrefixParser>();

			A.CallTo(() => defaultFlowLinePrefixParser.TryProcess(A<ICharacterStream>._, A<uint>._)).Returns(true);

			return new(
				separateInLineParser ?? defaultSeparateInLineParser,
				foldedLinesParser ?? defaultFoldedLineParser,
				flowLinePrefixParser ?? defaultFlowLinePrefixParser
			);
		}
	}
}
