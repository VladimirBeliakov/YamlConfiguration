using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class CommentParserTests
	{
		[Test]
		public async Task Process_StreamStartsWithDash_StreamAdvancesBehindComment()
		{
			var stream = new MemoryStream(new[] { '#', 'a', '\n', 'b' }.Select(c => (byte) c).ToArray());
			var charStream = getCharStream(stream);

			await new OneLineCommentParser().TryProcess(charStream);

			var actualCurrentStreamChar = await charStream.Peek();

			Assert.That(actualCurrentStreamChar, Is.EqualTo('b'));
		}

		[Test]
		public async Task Process_StreamDoesNotStartWithDash_StreamDoesNotAdvance()
		{
			var stream = new MemoryStream(new[] { 'a', '#', '\n', 'b' }.Select(c => (byte) c).ToArray());
			var charStream = getCharStream(stream);

			await new OneLineCommentParser().TryProcess(charStream);

			var actualCurrentStreamChar = await charStream.Peek();

			Assert.That(actualCurrentStreamChar, Is.EqualTo('a'));
		}

		private static ICharacterStream getCharStream(Stream stream) =>
			new TestStreamWrapper(new CharacterStream(stream));

		private class TestStreamWrapper : ICharacterStream
		{
			private readonly CharacterStream _charStream;

			public TestStreamWrapper(CharacterStream charStream)
			{
				_charStream = charStream;
			}

			public ValueTask<char> Peek()
			{
				throw new NotImplementedException();
			}

			public ValueTask<IReadOnlyCollection<char>> Peek(int charCount)
			{
				throw new NotImplementedException();
			}

			public ValueTask<char?> Read() => _charStream.Read();
		}
	}
}