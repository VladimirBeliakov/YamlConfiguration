using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class EnsureBreakAtEofCharacterStreamReaderTests
	{
		[Test]
		public async Task Read_StreamDidNotReachEof_ReturnsCharacterFromStream()
		{
			var expectedChars = new[] { 'a' };
			var stream = createStreamReaderFrom(expectedChars);

			var actualChars = new[]
			{
				await stream.Read(),
			};

			CollectionAssert.AreEqual(expectedChars, actualChars);
		}

		[Test]
		public async Task Read_StreamReachedEof_ReturnsCharsFromStreamAndBreak()
		{
			var chars = new[] { 'a' };
			var stream = createStreamReaderFrom(chars);

			var actualChars = new[]
			{
				await stream.Read(),
				await stream.Read(),
			};

			var expectedChars = chars.Append('\n');
			CollectionAssert.AreEqual(expectedChars, actualChars);
		}

		[Test]
		public async Task Read_ReadingAfterReachingEof_ReturnsNull()
		{
			var chars = new[] { 'a' };
			var stream = createStreamReaderFrom(chars);

			var actualChars = new[]
			{
				await stream.Read(),
				await stream.Read(),
				await stream.Read(),
			};

			var expectedChars = chars.Select(_ => (char?) _).Append('\n').Append(null);
			CollectionAssert.AreEqual(expectedChars, actualChars);
		}

		[Test]
		public async Task Read_StreamEndsWithBreak_NoAdditionalBreakAppended()
		{
			var chars = new[] { 'a', '\n' };
			var stream = createStreamReaderFrom(chars);

			var actualChars = new[]
			{
				await stream.Read(),
				await stream.Read(),
				await stream.Read(),
			};

			var expectedChars = chars.Select(_ => (char?) _).Append(null);
			CollectionAssert.AreEqual(expectedChars, actualChars);
		}

		private static EnsureBreakAtEofCharacterStreamReader createStreamReaderFrom(IEnumerable<char> chars)
		{
			return new(
				new CharacterStreamReader(
					new YamlCharacterStream(new MemoryStream(chars.Select(_ => (byte) _).ToArray()))
				)
			);
		}
	}
}