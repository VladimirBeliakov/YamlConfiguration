using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class CommentParserTests
	{
		[TestCaseSource(nameof(getWhiteAndCommentChars))]
		public async Task Process_StreamStartsWithWhiteCharsAndDashAndCommentChars_ReturnsTrueAndCallsReadLine(
			(string whiteChars, string commentChars) testCase
		)
		{
			var (whiteChars, commentChars) = testCase;
			var charStream = getCharStream($"{whiteChars}#{commentChars}");

			var result = await new OneLineCommentParser().TryProcess(charStream);

			Assert.True(result);
			A.CallTo(() => charStream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[TestCaseSource(nameof(getWhiteChars))]
		public async Task Process_StreamStartsWithWhiteCharsOnly_ReturnsFalseAndDoesNotCallReadLine(string whiteChars)
		{
			var charStream = getCharStream(whiteChars);

			var result = await new OneLineCommentParser().TryProcess(charStream);

			Assert.False(result);
			A.CallTo(() => charStream.ReadLine()).MustNotHaveHappened();
		}

		[Test]
		public void Process_StreamStartsWithTooManyWhiteChars_ReturnsFalseAndDoesNotCallReadLine()
		{
			var tooManyWhiteChars = CharStore.GetCharRange(" ") + " " + "#";
			var charStream = getCharStream(tooManyWhiteChars);

			Assert.ThrowsAsync<InvalidYamlException>(() => new OneLineCommentParser().TryProcess(charStream).AsTask());
		}

		[Test]
		public void Process_StreamStartsWithDashAndTooManyChars_Throws()
		{
			var tooManyCommentChars = CharStore.Repeat('a', Characters.CommentTextMaxLength + 1);
			var charStream = getCharStream($"#{tooManyCommentChars}");

			Assert.ThrowsAsync<InvalidYamlException>(() => new OneLineCommentParser().TryProcess(charStream).AsTask());
		}

		[Test]
		public async Task Process_StreamWithoutDash_ReturnsFalseAndDoesNotCallReadLine()
		{
			var charStream = getCharStream("a");

			var result = await new OneLineCommentParser().TryProcess(charStream);

			Assert.False(result);
			A.CallTo(() => charStream.ReadLine()).MustNotHaveHappened();
		}

		private static ICharacterStream getCharStream(string chars)
		{
			if (chars.Length == 0)
				throw new InvalidOperationException($"{nameof(chars)} must contain at least one char.");

			var charStream = A.Fake<ICharacterStream>();

			var requestedChars = 0;
			A.CallTo(() => charStream.Peek(A<int>._))
				.Invokes(o => requestedChars = (int) o.Arguments[0]!)
				.ReturnsLazily(() => chars.ToCharArray().Take(requestedChars).ToArray());

			A.CallTo(() => charStream.ReadLine()).Returns(chars);

			return charStream;
		}

		private static IEnumerable<(string whiteChars, string commentChars)> getWhiteAndCommentChars()
		{
			yield return (whiteChars: String.Empty, commentChars: String.Empty);
			yield return (
				whiteChars: CharStore.GetCharRange(" "),
				commentChars: CharStore.Repeat('a', Characters.CommentTextMaxLength)
			);
		}

		private static IEnumerable<string> getWhiteChars()
		{
			yield return " ";
			yield return "\t";
			yield return " \t";
			yield return CharStore.GetCharRange(" ");
		}
	}
}