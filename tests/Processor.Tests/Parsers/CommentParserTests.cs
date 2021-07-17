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
		[TestCaseSource(nameof(getCommentChars))]
		public async Task Process_StreamStartsWithDashAndCommentChars_ReturnsTrueAndCallsReadLine(string commentChars)
		{
			var charStream = getCharStream($"#{commentChars}");

			var result = await new OneLineCommentParser().TryProcess(charStream);

			Assert.True(result);
			A.CallTo(() => charStream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[TestCaseSource(nameof(getWhiteSpaces))]
		public async Task Process_StreamStartsWithWhiteSpaces_CallsReadsAsManyTimesAsWhiteSpaces(string whiteSpaces)
		{
			var charStream = getCharStream(whiteSpaces);

			await new OneLineCommentParser().TryProcess(charStream);

			A.CallTo(() => charStream.Read()).MustHaveHappened(numberOfTimes: whiteSpaces.Length, Times.Exactly);
		}

		[Test]
		public void Process_StreamStartsWithTooManyWhiteSpaces_Throws()
		{
			var tooManyWhiteSpaces = CharStore.GetCharRange(" ") + " ";
			var charStream = getCharStream(tooManyWhiteSpaces);

			Assert.ThrowsAsync<InvalidYamlException>(() => new OneLineCommentParser().TryProcess(charStream).AsTask());
		}

		[Test]
		public async Task Process_StreamStartsWithDashAndTooManyChars_Throws()
		{
			var tooManyCommentChars = CharStore.GetCharRange("a") + "a";
			var charStream = getCharStream($"#{tooManyCommentChars}");

			var result = await new OneLineCommentParser().TryProcess(charStream);

			Assert.True(result);
			A.CallTo(() => charStream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[Test]
		public async Task Process_StreamDoesNotStartWithDash_ReturnsFalseAndDoesNotCalReadLine()
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

			var thenConfiguration = A.CallTo(() => charStream.Peek()).Returns(chars.First()).Once();

			foreach (var @char in chars.Skip(1))
				thenConfiguration = thenConfiguration.Then.Returns(@char).Once();

			thenConfiguration.Then.Returns(null);

			return charStream;
		}

		private static IEnumerable<string> getCommentChars()
		{
			yield return String.Empty;
			yield return CharStore.GetCharRange("a");
		}

		private static IEnumerable<string> getWhiteSpaces()
		{
			yield return " ";
			yield return "\t";
			yield return " \t";
			yield return CharStore.GetCharRange(" ");
		}
	}
}