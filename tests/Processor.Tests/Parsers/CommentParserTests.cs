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
		[Test]
		public async Task Process_StreamStartsWithDash_ReturnsTrueAndCallsReadLine()
		{
			var charStream = getCharStream('#');

			var result = await new OneLineCommentParser().TryProcess(charStream);

			Assert.True(result);
			A.CallTo(() => charStream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[Test]
		public async Task Process_StreamDoesNotStartWithDash_ReturnsFalseAndDoesNotCalReadLine()
		{
			var charStream = getCharStream('a');

			var result = await new OneLineCommentParser().TryProcess(charStream);

			Assert.False(result);
			A.CallTo(() => charStream.ReadLine()).MustNotHaveHappened();
		}

		private static ICharacterStream getCharStream(char beginningChar)
		{
			var charStream = A.Fake<ICharacterStream>();

			A.CallTo(() => charStream.Peek()).Returns(beginningChar);

			return charStream;
		}
	}
}