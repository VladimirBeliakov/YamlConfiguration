using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class CommentParserTests
	{
		[TestCaseSource(nameof(getCommentChars))]
		public async Task Process_StreamStartsDashAndCommentChars_ReturnsTrue(
			string commentChars
		)
		{
			var charStream = getCharStream($"#{commentChars}");

			var result = await new CommentParser().TryProcess(charStream);

			Assert.True(result);
			A.CallTo(() => charStream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[Test]
		public async Task Process_StreamWithoutChars_ReturnsFalse()
		{
			var charStream = getCharStream(String.Empty);

			var result = await new CommentParser().TryProcess(charStream);

			Assert.False(result);
			charStream.AssertNotAdvanced();
		}

		[Test]
		public void Process_StreamStartsWithDashAndTooManyChars_Throws()
		{
			var tooManyCommentChars = CharStore.Repeat('a', Characters.CommentTextMaxLength + 1);
			var charStream = getCharStream($"#{tooManyCommentChars}");

			Assert.ThrowsAsync<InvalidYamlException>(() => new CommentParser().TryProcess(charStream).AsTask());
		}

		[Test]
		public async Task Process_StreamWithoutDash_ReturnsFalse()
		{
			var charStream = getCharStream("a");

			var result = await new CommentParser().TryProcess(charStream);

			Assert.False(result);
			charStream.AssertNotAdvanced();
		}

		private static ICharacterStream getCharStream(string chars)
		{
			var charStream = A.Fake<ICharacterStream>();

			var peekedChar = chars.Length > 0 ? chars[0] : (char?) null;

			A.CallTo(() => charStream.Peek()).Returns(peekedChar);

			A.CallTo(() => charStream.ReadLine()).Returns(chars);

			return charStream;
		}

		private static IEnumerable<string> getCommentChars()
		{
			yield return String.Empty;
			yield return CharStore.Repeat('a', Characters.CommentTextMaxLength);
		}
	}
}