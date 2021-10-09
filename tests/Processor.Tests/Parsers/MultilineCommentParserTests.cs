using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class MultilineCommentParserTests
	{
		[Test]
		public async Task TryProcess_NotAtStartOfLineAndCommentParserReturnsFalse_ReturnsFalse()
		{
			var stream = A.Fake<ICharacterStream>();
			var commentParser = A.Fake<ICommentParser>();
			A.CallTo(() => stream.IsAtStartOfLine).Returns(false);
			A.CallTo(() => commentParser.TryProcess(stream, false)).Returns(false);

			var result = await createParser(commentParser).TryProcess(stream);

			Assert.False(result);
			A.CallTo(() => commentParser.TryProcess(stream, true)).MustNotHaveHappened();
		}

		[Test]
		public async Task TryProcess_NotAtStartOfLineAndCommentParserReturnsTrue_ReturnsTrue()
		{
			var stream = A.Fake<ICharacterStream>();
      var commentParser = A.Fake<ICommentParser>();
			A.CallTo(() => stream.IsAtStartOfLine).Returns(false);
			A.CallTo(() => commentParser.TryProcess(stream, false)).Returns(true);

			var result = await createParser(commentParser).TryProcess(stream);

			Assert.True(result);
			A.CallTo(() => commentParser.TryProcess(stream, true)).MustHaveHappened();
		}

		[Test]
		public async Task TryProcess_AtStartOfLine_ReturnsTrue()
		{
			var stream = A.Fake<ICharacterStream>();
      var commentParser = A.Fake<ICommentParser>();
			A.CallTo(() => stream.IsAtStartOfLine).Returns(true);

			var result = await createParser(commentParser).TryProcess(stream);

			Assert.True(result);
			A.CallTo(() => commentParser.TryProcess(stream, true)).MustHaveHappened();
		}

		private static MultilineCommentParser createParser(ICommentParser? commentParser = null) =>
			new(commentParser ?? A.Fake<ICommentParser>());
	}
}
