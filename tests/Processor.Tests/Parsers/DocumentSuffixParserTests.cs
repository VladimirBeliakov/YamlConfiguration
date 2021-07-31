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
		public async Task Process_StreamWithLessThanThreeChars_ReturnsFalseAndDoesNotCallReadAndReadLine(int charCount)
		{
			var chars = CharStore.Repeat('.', charCount);
			var stream = getCharStream(chars);

			var result = await createParser().Process(stream);

			Assert.False(result);
			A.CallTo(() => stream.Read()).MustNotHaveHappened();
			A.CallTo(() => stream.ReadLine()).MustNotHaveHappened();
		}

		[TestCase("a..")]
		[TestCase("aa.")]
		[TestCase("aaa")]
		[TestCase(".a.")]
		[TestCase(".aa")]
		[TestCase("..a")]
		public async Task Process_SomeCharsInStreamIsNotDot_ReturnsFalseAndDoesNotCallReadAndReadLine(string chars)
		{
			var stream = getCharStream(chars);

			var result = await createParser().Process(stream);

			Assert.False(result);
			A.CallTo(() => stream.Read()).MustNotHaveHappened();
			A.CallTo(() => stream.ReadLine()).MustNotHaveHappened();
		}

		[Test]
		public async Task Process_StreamWithOnlyThreeDots_ReturnsTrueAndCallsReadLineButNotRead()
		{
			var stream = getCharStream("...");

			var result = await createParser().Process(stream);

			Assert.True(result);
			A.CallTo(() => stream.Read()).MustNotHaveHappened();
			A.CallTo(() => stream.ReadLine()).MustHaveHappened();
		}

		[Test]
		public async Task Process_UnacceptableCharFollowsThreeDots_ReturnsFalseAndDoesNotCallReadAndReadLine()
		{
			var stream = getCharStream("...a");

			var result = await createParser().Process(stream);

			Assert.False(result);
			A.CallTo(() => stream.Read()).MustNotHaveHappened();
			A.CallTo(() => stream.ReadLine()).MustNotHaveHappened();
		}

		[TestCase("... ")]
		[TestCase("...\t")]
		public void Process_LineWithDocumentEndContainsInvalidComment_ThrowsInvalidYamlException(string chars)
		{
			var stream = getCharStream(chars);
			var oneLineCommentParser = A.Fake<IOneLineCommentParser>();
			A.CallTo(() => oneLineCommentParser.TryProcess(stream)).Returns(false);

			Assert.ThrowsAsync<InvalidYamlException>(() => createParser(oneLineCommentParser).Process(stream).AsTask());
		}

		[TestCase("... ")]
		[TestCase("...\t")]
		public void Process_LineWithDocumentEndContainsValidComment_DoesNotThrowInvalidYamlException(string chars)
		{
			var stream = getCharStream(chars);
			var oneLineCommentParser = A.Fake<IOneLineCommentParser>();
			A.CallTo(() => oneLineCommentParser.TryProcess(stream)).Returns(true);

			Assert.DoesNotThrow(() => createParser(oneLineCommentParser).Process(stream).AsTask());
		}

		[Test]
		public async Task Process_LineWithDocumentEndEndsWithLineBreak_DoesNotCallOneLineCommentParser()
		{
			var stream = getCharStream("...\n");
			var oneLineCommentParser = A.Fake<IOneLineCommentParser>();

			await createParser(oneLineCommentParser).Process(stream);

			A.CallTo(() => oneLineCommentParser.TryProcess(stream)).MustNotHaveHappened();
		}

		[TestCase("... ")]
		[TestCase("...\t")]
		[TestCase("...\n")]
		public async Task Process_StreamWithValidDocumentEnd_CallsMultiLineCommentParser(string chars)
		{
			var stream = getCharStream(chars);
			var multiLineCommentParser = A.Fake<IMultiLineCommentParser>();

			await createParser(multiLineCommentParser: multiLineCommentParser).Process(stream);

			A.CallTo(() => multiLineCommentParser.Process(stream)).MustHaveHappened();
		}

		[TestCase("... ")]
		[TestCase("...\t")]
		[TestCase("...\n")]
		public async Task Process_StreamWithValidDocumentEnd_ReturnsTrueAndCallsMultiLineCommentParserAndRead(
			string chars
		)
		{
			var stream = getCharStream(chars);
			var multiLineCommentParser = A.Fake<IMultiLineCommentParser>();

			var result = await createParser(multiLineCommentParser: multiLineCommentParser).Process(stream);

			Assert.True(result);
			A.CallTo(() => multiLineCommentParser.Process(stream)).MustHaveHappened();
			A.CallTo(() => stream.Read(4)).MustHaveHappened();
		}

		private static ICharacterStream getCharStream(string chars)
		{
			var charStream = A.Fake<ICharacterStream>();

			A.CallTo(() => charStream.Peek(A<int>._)).Returns(chars.ToCharArray());

			return charStream;
		}

		private static DocumentSuffixParser createParser(
			IOneLineCommentParser? oneLineCommentParser = null,
			IMultiLineCommentParser? multiLineCommentParser = null
		)
		{
			var defaultOneLineCommentParser = A.Fake<IOneLineCommentParser>();

			A.CallTo(() => defaultOneLineCommentParser.TryProcess(A<ICharacterStream>._)).Returns(true);

			return new DocumentSuffixParser(
				oneLineCommentParser ?? defaultOneLineCommentParser,
				multiLineCommentParser ?? A.Dummy<IMultiLineCommentParser>()
			);
		}
	}
}