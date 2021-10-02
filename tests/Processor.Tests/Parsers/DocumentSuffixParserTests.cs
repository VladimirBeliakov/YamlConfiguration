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
		public void Process_MultiCommentParserReturnsFalse_Throws()
		{
			var stream = getCharStream("...");
			var multiLineCommentParser = A.Fake<IMultiLineCommentParser>();
			A.CallTo(() => multiLineCommentParser.TryProcess(stream)).Returns(false);

			var parser = createParser(multiLineCommentParser);
			Assert.ThrowsAsync<InvalidYamlException>(() => parser.Process(stream).AsTask());
		}

		[Test]
		public async Task Process_MultiLineCommentParserReturnsTrue_ReturnsTrue()
		{
			var stream = getCharStream("...");
			var multiLineCommentParser = A.Fake<IMultiLineCommentParser>();
			A.CallTo(() => multiLineCommentParser.TryProcess(stream)).Returns(true);

			var result = await createParser(multiLineCommentParser).Process(stream);

			Assert.True(result);
			A.CallTo(() => stream.Read(3)).MustHaveHappened();
		}

		private static ICharacterStream getCharStream(string chars)
		{
			var charStream = A.Fake<ICharacterStream>();

			A.CallTo(() => charStream.Peek(A<uint>._)).Returns(chars.ToCharArray());

			return charStream;
		}

		private static DocumentSuffixParser createParser(IMultiLineCommentParser? multiLineCommentParser = null) =>
			new(multiLineCommentParser ?? A.Dummy<IMultiLineCommentParser>());
	}
}