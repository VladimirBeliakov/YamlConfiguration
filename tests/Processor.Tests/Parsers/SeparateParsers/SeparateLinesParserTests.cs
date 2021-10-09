using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class SeparateLinesParserTests
	{
		[Test]
		public async Task TryProcess_WithIndentButNoComment_ReturnsFalse()
		{
			var stream = A.Fake<ICharacterStream>();
			var multilineCommentParser = A.Fake<IMultilineCommentParser>();
			A.CallTo(() => multilineCommentParser.TryProcess(stream)).Returns(false);

			var result = await createParser(multilineCommentParser).TryProcess(stream, indentLength: 0);

			Assert.False(result);
		}

		[Test]
		public async Task TryProcess_WithIndentAndCommentButNoFlowLinePrefix_ReturnsFalse()
		{
			var stream = A.Fake<ICharacterStream>();
			var multilineCommentParser = A.Fake<IMultilineCommentParser>();
			var flowLinePrefixParser = A.Fake<IFlowLinePrefixParser>();
			A.CallTo(() => multilineCommentParser.TryProcess(stream)).Returns(true);
			A.CallTo(() => flowLinePrefixParser.TryProcess(stream, 0)).Returns(false);

			var result =
				await createParser(multilineCommentParser, flowLinePrefixParser).TryProcess(stream, indentLength: 0);

			Assert.False(result);
		}

		[Test]
		public async Task TryProcess_WithIndentAndCommentAndFlowLinePrefix_ReturnsTrue()
		{
			var stream = A.Fake<ICharacterStream>();
			var multilineCommentParser = A.Fake<IMultilineCommentParser>();
			var flowLinePrefixParser = A.Fake<IFlowLinePrefixParser>();
			A.CallTo(() => multilineCommentParser.TryProcess(stream)).Returns(true);
			A.CallTo(() => flowLinePrefixParser.TryProcess(stream, 0)).Returns(true);

			var result =
				await createParser(multilineCommentParser, flowLinePrefixParser).TryProcess(stream, indentLength: 0);

			Assert.True(result);
		}

		[Test]
		public async Task TryProcess_NoIndentAndNoSeparateInLine_ReturnsFalse()
		{
			var stream = A.Fake<ICharacterStream>();
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			A.CallTo(() => separateInLineParser.Peek(stream)).Returns(
				new ParsedSeparateInLineResult(isSeparateInLine: false, whiteSpaceCount: 0U)
			);

			var result = await createParser(separateInLineParser: separateInLineParser).TryProcess(stream);

			Assert.False(result);
		}

		[Test]
		public async Task TryProcess_NoIndentButWithSeparateInLine_ReturnsTrue()
		{
			var stream = A.Fake<ICharacterStream>();
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			A.CallTo(() => separateInLineParser.Peek(stream)).Returns(
				new ParsedSeparateInLineResult(isSeparateInLine: true, whiteSpaceCount: 0U)
			);

			var result = await createParser(separateInLineParser: separateInLineParser).TryProcess(stream);

			Assert.True(result);
		}

		[Test]
		public async Task TryProcess_NoIndentButWithSeparateInLineAndZeroWhiteSpaces_DoesNotAdvanceStream()
		{
			var stream = A.Fake<ICharacterStream>();
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			A.CallTo(() => separateInLineParser.Peek(stream)).Returns(
				new ParsedSeparateInLineResult(isSeparateInLine: true, whiteSpaceCount: 0U)
			);

			await createParser(separateInLineParser: separateInLineParser).TryProcess(stream);

			A.CallTo(() => stream.Read(A<uint>._)).MustNotHaveHappened();
		}

		[TestCase(1U)]
		[TestCase(2U)]
		public async Task TryProcess_NoIndentButWithSeparateInLine_AdvancesStreamByWhiteSpaceCount(uint whiteSpaceCount)
		{
			var stream = A.Fake<ICharacterStream>();
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			A.CallTo(() => separateInLineParser.Peek(stream)).Returns(
				new ParsedSeparateInLineResult(isSeparateInLine: true, whiteSpaceCount)
			);

			await createParser(separateInLineParser: separateInLineParser).TryProcess(stream);

			A.CallTo(() => stream.Read(whiteSpaceCount)).MustHaveHappenedOnceExactly();
		}

		private static SeparateLinesParser createParser(
			IMultilineCommentParser? multilineCommentParser = null,
			IFlowLinePrefixParser? flowLinePrefixParser = null,
			ISeparateInLineParser? separateInLineParser = null
		)
		{
			var defaultMultilineCommentParser = A.Fake<IMultilineCommentParser>();
			var defaultFlowLinePrefixParser = A.Fake<IFlowLinePrefixParser>();
			var defaultSeparateInLineParser = A.Fake<ISeparateInLineParser>();

			A.CallTo(() => defaultMultilineCommentParser.TryProcess(A<ICharacterStream>._)).Returns(true);
			A.CallTo(() => defaultFlowLinePrefixParser.TryProcess(A<ICharacterStream>._, A<uint>._)).Returns(true);
			A.CallTo(() => defaultSeparateInLineParser.Peek(A<ICharacterStream>._)).Returns(
				new ParsedSeparateInLineResult(isSeparateInLine: true, whiteSpaceCount: 1)
			);

			return new(
				multilineCommentParser ?? defaultMultilineCommentParser,
				flowLinePrefixParser ?? defaultFlowLinePrefixParser,
				separateInLineParser ?? defaultSeparateInLineParser
			);
		}
	}
}