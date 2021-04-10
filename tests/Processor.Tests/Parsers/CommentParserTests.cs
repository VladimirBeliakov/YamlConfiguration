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

			await new CommentParser().Process(charStream);

			var actualCurrentStreamChar = await charStream.Peek();

			Assert.That(actualCurrentStreamChar, Is.EqualTo('b'));
		}

		[Test]
		public async Task Process_StreamDoesNotStartWithDash_StreamDoesNotAdvance()
		{
			var stream = new MemoryStream(new[] { 'a', '#', '\n', 'b' }.Select(c => (byte) c).ToArray());
			var charStream = getCharStream(stream);

			await new CommentParser().Process(charStream);

			var actualCurrentStreamChar = await charStream.Peek();

			Assert.That(actualCurrentStreamChar, Is.EqualTo('a'));
		}

		[Test]
		public async Task Process_StreamDoesNotEndWithLineBreak_StreamIsDisposed()
		{
			var stream = new MemoryStream(new[] { '#', 'a', 'b' }.Select(c => (byte) c).ToArray());
			var charStream = getCharStream(stream);

			await new CommentParser().Process(charStream);

			Assert.True(charStream.IsDisposed);
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

			public bool IsDisposed => _charStream.IsDisposed;

			public ValueTask<char> Peek() => _charStream.Peek();

			public ValueTask<char?> Read() => _charStream.Read();
		}
	}
}