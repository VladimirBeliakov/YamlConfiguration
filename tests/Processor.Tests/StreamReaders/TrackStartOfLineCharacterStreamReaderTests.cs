using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture]
	public class TrackStartOfLineCharacterStreamReaderTests
	{
		[Test]
		public void IsAtStartOfLine_JustCreated_ReturnsTrue()
		{
			var streamReader = createStreamReaderFrom(Array.Empty<char>());

			Assert.True(streamReader.IsAtStartOfLine);
		}

		[Test]
		public async Task IsAtStartOfLine_PeekChar_ReturnsTrue()
		{
			var streamReader = createStreamReaderFrom(Array.Empty<char>());

			await streamReader.Peek(1);

			Assert.True(streamReader.IsAtStartOfLine);
		}

		[Test]
		public async Task IsAtStartOfLine_ReadNotBreakCharThenPeekChar_ReturnsFalse()
		{
			var streamReader = createStreamReaderFrom(new[] { 'a' });

			await streamReader.Read();
			await streamReader.Peek(1);

			Assert.False(streamReader.IsAtStartOfLine);
		}

		[Test]
		public async Task IsAtStartOfLine_PeekLine_ReturnsTrue()
		{
			var streamReader = createStreamReaderFrom(Array.Empty<char>());

			await streamReader.PeekLine();

			Assert.True(streamReader.IsAtStartOfLine);
		}

		[Test]
		public async Task IsAtStartOfLine_ReadNotBreakCharThenPeekLine_ReturnsFalse()
		{
			var streamReader = createStreamReaderFrom(new[] { 'a' });

			await streamReader.Read();
			await streamReader.PeekLine();

			Assert.False(streamReader.IsAtStartOfLine);
		}

		[Test]
		public async Task IsAtStartOfLine_ReadNotBreakCharThenBreakChar_ReturnsTrue()
		{
			var streamReader = createStreamReaderFrom(new[] { 'a', '\n' });

			await streamReader.Read();
			await streamReader.Read();

			Assert.True(streamReader.IsAtStartOfLine);
		}

		[Test]
		public async Task IsAtStartOfLine_ReadBreakCharThenNotBreakChar_ReturnsFalse()
		{
			var streamReader = createStreamReaderFrom(new[] { '\n', 'a' });

			await streamReader.Read();
			await streamReader.Read();

			Assert.False(streamReader.IsAtStartOfLine);
		}

		[Test]
		public async Task IsAtStartOfLine_ReadNotBreakCharThenReadLine_ReturnsTrue()
		{
			var streamReader = createStreamReaderFrom(new[] { 'a', '\n' });

			await streamReader.Read();
			await streamReader.ReadLine();

			Assert.True(streamReader.IsAtStartOfLine);
		}

		[Test]
		public async Task IsAtStartOfLine_ReadBreakCharThenReadLine_ReturnsTrue()
		{
			var streamReader = createStreamReaderFrom(new[] { '\n', 'a', '\n' });

			await streamReader.Read();
			await streamReader.ReadLine();

			Assert.True(streamReader.IsAtStartOfLine);
		}

		private static TrackStartOfLineCharacterStreamReader createStreamReaderFrom(IEnumerable<char> chars)
		{
			return new(
				new BufferedCharacterStreamReader(
					new EnsureBreakAtEofCharacterStreamReader(
						new CharacterStreamReader(
							new YamlCharacterStream(new MemoryStream(chars.Select(_ => (byte) _).ToArray()))
						)
					)
				)
			);
		}
	}
}