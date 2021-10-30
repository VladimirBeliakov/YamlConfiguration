using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class FlowFoldedLinesParserTests
	{
		[Test]
		public async Task TryProcess_StreamWithoutFoldedLines_ReturnsNull()
		{
			var foldedLineParser = A.Fake<IFoldedLinesParser>();
			A.CallTo(() => foldedLineParser.Process(A<ICharacterStream>._)).Returns(null);

			var result =
				await createParser(foldedLineParser).TryProcess(A.Dummy<ICharacterStream>(), indentLength: 0);

			Assert.Null(result);
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
				() => parser.TryProcess(A.Dummy<ICharacterStream>(), indentLength: 0).AsTask()
			);
		}

		[Test]
		public async Task TryProcess_StreamWithFoldedLinesAndFlowLinePrefix_ReturnsFoldedLinesResult()
		{
			var foldedLinesResult = new FoldedLinesResult(emptyLineCount: 1);
			var foldedLineParser = A.Fake<IFoldedLinesParser>();
			A.CallTo(() => foldedLineParser.Process(A<ICharacterStream>._)).Returns(foldedLinesResult);
			var flowLinePrefixParser = A.Fake<IFlowLinePrefixParser>();
			A.CallTo(() => flowLinePrefixParser.TryProcess(A<ICharacterStream>._, A<uint>._)).Returns(true);

			var parser = createParser(foldedLineParser, flowLinePrefixParser);
			var result = await parser.TryProcess(A.Dummy<ICharacterStream>(), indentLength: 0);

			Assert.That(result, Is.SameAs(foldedLinesResult));
		}

		private static FlowFoldedLinesParser createParser(
			IFoldedLinesParser? foldedLinesParser = null,
			IFlowLinePrefixParser? flowLinePrefixParser = null
		)
		{
			var separateInLineParser = A.Dummy<ISeparateInLineParser>();
			var defaultFoldedLineParser = A.Fake<IFoldedLinesParser>();

			A.CallTo(() => defaultFoldedLineParser.Process(A<ICharacterStream>._))
				.Returns(new FoldedLinesResult(emptyLineCount: 1));

			var defaultFlowLinePrefixParser = A.Fake<IFlowLinePrefixParser>();

			A.CallTo(() => defaultFlowLinePrefixParser.TryProcess(A<ICharacterStream>._, A<uint>._)).Returns(true);

			return new(
				separateInLineParser,
				foldedLinesParser ?? defaultFoldedLineParser,
				flowLinePrefixParser ?? defaultFlowLinePrefixParser
			);
		}
	}
}
