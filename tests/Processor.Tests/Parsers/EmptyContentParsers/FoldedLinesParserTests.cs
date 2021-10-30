using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class FoldedLinesParserTests
	{
		[Test]
		public async Task Process_StreamDoesNotStartWithBreak_ReturnsNull()
		{
			var stream = createStreamFrom('a');

			var result = await createParser().Process(stream);

			Assert.Null(result);
			stream.AssertNotAdvanced();
		}

		[Test]
		public async Task Process_EmptyLineParserReturnsZero_ReturnsZeroEmptyLinesAndBreakAsSpace()
		{
			var stream = createStreamFrom('\n');
			var emptyLineParser = A.Fake<IEmptyLineParser>();
			A.CallTo(() => emptyLineParser.TryProcess(A<ICharacterStream>._)).Returns(false);

			var result = await createParser(emptyLineParser).Process(stream);

			Assert.Multiple(() =>
				{
					Assert.That(result?.EmptyLineCount, Is.Zero);
					Assert.True(result?.IsBreakAsSpace);
				}
			);
			A.CallTo(() => stream.Read(1)).MustHaveHappenedOnceExactly();
		}

		[TestCase(1)]
		[TestCase(2)]
		public async Task Process_EmptyLineParserReturnsNotZero_ReturnsNotZeroEmptyLinesAndNotBreakAsSpace(
			int zeroCount
		)
		{
			var stream = createStreamFrom('\n');
			var emptyLineParser = A.Fake<IEmptyLineParser>();
			A.CallTo(() => emptyLineParser.TryProcess(A<ICharacterStream>._)).Returns(true).NumberOfTimes(zeroCount);

			var result = await createParser(emptyLineParser).Process(stream);

			Assert.Multiple(() =>
				{
					Assert.That(result?.EmptyLineCount, Is.EqualTo(zeroCount));
					Assert.False(result?.IsBreakAsSpace);
				}
			);
			A.CallTo(() => stream.Read(1)).MustHaveHappenedOnceExactly();
		}

		private static ICharacterStream createStreamFrom(char firstChar = '\n')
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.Peek()).Returns(firstChar);

			return stream;
		}

		private static FoldedLinesParser createParser(
			IEmptyLineParser? emptyLineParser = null
		)
		{
			var defaultEmptyLineParser = A.Fake<IEmptyLineParser>();

			A.CallTo(() => defaultEmptyLineParser.TryProcess(A<ICharacterStream>._)).Returns(true).Once();

			return new(emptyLineParser ?? defaultEmptyLineParser);
		}
	}
}
