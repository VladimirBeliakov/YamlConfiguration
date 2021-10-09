using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture]
	public class DocumentSuffixParserTests
	{
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		public async Task Process_StreamWithLessThanThreeChars_ReturnsFalse(int charCount)
		{
			var chars = CharStore.Repeat('.', charCount);
			var stream = getCharStream(chars);

			var result = await createParser().Process(stream);

			Assert.False(result);
			stream.AssertNotAdvanced();
		}

		[TestCase("a..")]
		[TestCase("aa.")]
		[TestCase("aaa")]
		[TestCase(".a.")]
		[TestCase(".aa")]
		[TestCase("..a")]
		public async Task Process_SomeCharsInStreamIsNotDot_ReturnsFalse(string chars)
		{
			var stream = getCharStream(chars);

			var result = await createParser().Process(stream);

			Assert.False(result);
			stream.AssertNotAdvanced();
		}

		[Test]
		public void Process_CommentParserReturnsFalse_Throws()
		{
			var stream = getCharStream("...");
			var commentParser = A.Fake<ICommentParser>();
			A.CallTo(() => commentParser.TryProcess(stream, false)).Returns(false);

			var parser = createParser(commentParser);
			Assert.ThrowsAsync<InvalidYamlException>(() => parser.Process(stream).AsTask());
		}

		[Test]
		public async Task Process_CommentParserReturnsTrue_ReturnsTrue()
		{
			var stream = getCharStream("...");
			var commentParser = A.Fake<ICommentParser>();
			A.CallTo(() => commentParser.TryProcess(stream, false)).Returns(true);

			var result = await createParser(commentParser).Process(stream);

			Assert.True(result);
			A.CallTo(() => stream.Read(3)).MustHaveHappenedOnceExactly();
			A.CallTo(() => commentParser.TryProcess(stream, true)).MustHaveHappened();
		}

		private static ICharacterStream getCharStream(string chars)
		{
			var charStream = A.Fake<ICharacterStream>();

			A.CallTo(() => charStream.Peek(A<uint>._)).Returns(chars.ToCharArray());

			return charStream;
		}

		private static DocumentSuffixParser createParser(ICommentParser? commentParser = null) =>
			new(commentParser ?? A.Dummy<ICommentParser>());
	}
}