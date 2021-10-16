using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class SeparateParserTests
	{
		[TestCaseSource(nameof(getInAndOutContext))]
		public async Task TryProcess_InAndOutContextButNoSeparateLines_ReturnsFalse(Context context)
		{
			var stream = A.Fake<ICharacterStream>();
			var separateLinesParser = A.Fake<ISeparateLinesParser>();
			A.CallTo(() => separateLinesParser.TryProcess(stream, null)).Returns(false);

			var result = await createParser(separateLinesParser).TryProcess(stream, context);

			Assert.False(result);
		}

		[TestCaseSource(nameof(getInAndOutContext))]
		public async Task TryProcess_InAndOutContextWithSeparateLines_ReturnsTrue(Context context)
		{
			var stream = A.Fake<ICharacterStream>();
			var separateLinesParser = A.Fake<ISeparateLinesParser>();
			A.CallTo(() => separateLinesParser.TryProcess(stream, null)).Returns(true);

			var result = await createParser(separateLinesParser).TryProcess(stream, context);

			Assert.True(result);
		}

		[TestCaseSource(nameof(getKeyContext))]
		public async Task TryProcess_KeyContextButNoSeparateLines_ReturnsFalse(Context context)
		{
			var stream = A.Fake<ICharacterStream>();
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			A.CallTo(() => separateInLineParser.Peek(stream)).Returns(
				new ParsedSeparateInLineResult(isSeparateInLine: false, whiteSpaceCount: 0)
			);

			var result = await createParser(separateInLineParser: separateInLineParser).TryProcess(stream, context);

			Assert.False(result);
		}

		[TestCaseSource(nameof(getKeyContext))]
		public async Task TryProcess_KeyContextWithSeparateLines_ReturnsTrue(Context context)
		{
			var stream = A.Fake<ICharacterStream>();
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			A.CallTo(() => separateInLineParser.Peek(stream)).Returns(
				new ParsedSeparateInLineResult(isSeparateInLine: true, whiteSpaceCount: 0)
			);

			var result = await createParser(separateInLineParser: separateInLineParser).TryProcess(stream, context);

			Assert.True(result);
		}

		private static SeparateParser createParser(
			ISeparateLinesParser? separateLinesParser = null,
			ISeparateInLineParser? separateInLineParser = null
		)
		{
			var defaultSeparateLinesParser = A.Fake<ISeparateLinesParser>();
			var defaultSeparateInLineParser = A.Fake<ISeparateInLineParser>();
			A.CallTo(() => defaultSeparateLinesParser.TryProcess(A<ICharacterStream>._, A<uint?>._)).Returns(true);
			A.CallTo(() => defaultSeparateInLineParser.Peek(A<ICharacterStream>._)).Returns(
				new ParsedSeparateInLineResult(isSeparateInLine: true, whiteSpaceCount: 1)
			);

			return new(
				separateLinesParser ?? defaultSeparateLinesParser,
				separateInLineParser ?? defaultSeparateInLineParser
			);
		}

		private static IEnumerable<Context> getInAndOutContext() => Enum.GetValues<Context>().Except(getKeyContext());

		private static IEnumerable<Context> getKeyContext()
		{
			yield return Context.BlockKey;
			yield return Context.FlowKey;
		}
	}
}