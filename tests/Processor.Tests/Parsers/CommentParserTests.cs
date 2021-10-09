using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class CommentParserTests
	{
		[Test]
		public async Task TryProcess_LineCommentAndNoSeparateInLine_ReturnsFalse()
		{
			var charStream = getCharStream("#abc");

			var result = await createParser(withSeparateInLine: false).TryProcess(charStream, isLineComment: true);

			Assert.False(result);
			charStream.AssertNotAdvanced();
		}

		[Test]
		public async Task TryProcess_NoCharsAndNoWhiteSpaceInSeparateInLine_ReturnsTrueAndDoesNotAdvanceStream()
		{
			var charStream = getCharStream(String.Empty, whiteSpaceCount: 0);

			var result = await createParser(whiteSpaceCount: 0).TryProcess(charStream);

			Assert.True(result);
			charStream.AssertNotAdvanced();
		}

		[TestCase(1U)]
		[TestCase(2U)]
		public async Task TryProcess_NoCharsButWhiteSpacesInSeparateInLine_ReturnsTrue(
			uint whiteSpaceCount
		)
		{
			var charStream = getCharStream(String.Empty, whiteSpaceCount);

			var result = await createParser(whiteSpaceCount: whiteSpaceCount).TryProcess(charStream);

			Assert.True(result);
			A.CallTo(() => charStream.Read(whiteSpaceCount)).MustHaveHappenedOnceExactly();
		}

		[TestCase(0U)]
		[TestCase(1U)]
		[TestCase(2U)]
		public async Task TryProcess_BreakAndWhiteSpacesInSeparateInLine_ReturnsTrue(uint whiteSpaceCount)
		{
			var charStream = getCharStream("\n", whiteSpaceCount);

			var result = await createParser(whiteSpaceCount: whiteSpaceCount).TryProcess(charStream);

			Assert.True(result);
			A.CallTo(() => charStream.Read(whiteSpaceCount + 1)).MustHaveHappenedOnceExactly();
		}

		[Test]
		public async Task TryProcess_NoSeparateInLineButCommentChar_ReturnsFalse()
		{
			var charStream = getCharStream("#");

			var result = await createParser(withSeparateInLine: false).TryProcess(charStream);

			Assert.False(result);
			charStream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getCommentChars))]
		public async Task TryProcess_SeparateInLineAndDashAndCommentChars_ReturnsTrue(string commentChars)
		{
			var charStream = getCharStream($"#{commentChars}");

			var result = await createParser(withSeparateInLine: true).TryProcess(charStream);

			Assert.True(result);
			A.CallTo(() => charStream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[TestCase(0U)]
		[TestCase(1U)]
		[TestCase(2U)]
		public void TryProcess_WhiteSpacesAndDashAndTooManyCommentChars_Throws(uint whiteSpaceCount)
		{
			var tooManyCommentChars = CharStore.Repeat('a', Characters.CommentTextMaxLength + 1);
			var charStream = getCharStream($"#{tooManyCommentChars}", whiteSpaceCount);

			Assert.ThrowsAsync<InvalidYamlException>(
				() => createParser(whiteSpaceCount: whiteSpaceCount).TryProcess(charStream).AsTask()
			);
		}

		[Test]
		public async Task TryProcess_NoDashOrBreakOrEmptyStream_ReturnsFalse()
		{
			var charStream = getCharStream("a");

			var result = await createParser().TryProcess(charStream);

			Assert.False(result);
			charStream.AssertNotAdvanced();
		}

		private static ICharacterStream getCharStream(string chars, uint whiteSpaceCount = 0)
		{
			var charStream = A.Fake<ICharacterStream>();

			var whiteSpaces = new String(Enumerable.Repeat(' ', (int) whiteSpaceCount).ToArray());
			var peekedChar = chars.Length > 0 ? chars[0] : (char?) null;

			A.CallTo(() => charStream.Peek(A<uint>._)).Returns(
				peekedChar.HasValue ? new List<char>(whiteSpaces){ peekedChar.Value } : new List<char>(whiteSpaces)
			);

			A.CallTo(() => charStream.ReadLine()).Returns($"{whiteSpaces}{chars}");

			return charStream;
		}

		private static IEnumerable<string> getCommentChars()
		{
			yield return String.Empty;
			yield return CharStore.Repeat('a', Characters.CommentTextMaxLength);
		}

		private static CommentParser createParser(bool withSeparateInLine = true, uint whiteSpaceCount = 0)
		{
			var separateInLineParser = A.Fake<ISeparateInLineParser>();

			A.CallTo(() => separateInLineParser.Peek(A<ICharacterStream>._)).Returns(
				new ParsedSeparateInLineResult(withSeparateInLine, whiteSpaceCount)
			);

			return new(separateInLineParser);
		}
	}
}