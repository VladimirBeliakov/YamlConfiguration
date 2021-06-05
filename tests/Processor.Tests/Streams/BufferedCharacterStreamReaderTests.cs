using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class BufferedCharacterStreamReaderTests
	{
		[TestCase(1, new[] { 'a' })]
		[TestCase(2, new[] { 'a', 'b' })]
		public async Task PickChars_ReturnsCorrectChar(int charCount, char[] expectedChars)
		{
			var charArray = new[] { 'a', 'b', 'c' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			var peekedChars = await bufferedStreamReader.Peek(charCount);

			CollectionAssert.AreEqual(expectedChars, peekedChars);
		}

		[TestCase(1, new[] { 'a' })]
		[TestCase(2, new[] { 'a', 'b' })]
		public async Task ReadChars_ReturnsCorrectChars(int charCount, char[] expectedChars)
		{
			var charArray = new[] { 'a', 'b', 'c' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			var readChars = new char?[charCount];

			for (var i = 0; i < charCount; i++)
				readChars[i] = await bufferedStreamReader.Read();

			CollectionAssert.AreEqual(expectedChars, readChars);
		}

		[Test]
		public async Task PeekTwoCharsAndThenReadTwoChars_PeekDoesNotAdvanceStream()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			await bufferedStreamReader.Peek(2);
			var readCharOne = await bufferedStreamReader.Read();
			var readCharTwo = await bufferedStreamReader.Read();

			Assert.Multiple(() =>
				{
					Assert.That(readCharOne, Is.EqualTo('a'));
					Assert.That(readCharTwo, Is.EqualTo('b'));
				}
			);
		}

		[Test]
		public async Task PeekOneCharsAndThenReadTwoChar_PeekDoesNotAdvanceStream()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			await bufferedStreamReader.Peek(1);
			var readCharOne = await bufferedStreamReader.Read();
			var readCharTwo = await bufferedStreamReader.Read();

			Assert.Multiple(() =>
				{
					Assert.That(readCharOne, Is.EqualTo('a'));
					Assert.That(readCharTwo, Is.EqualTo('b'));
				}
			);
		}

		[Test]
		public async Task PeekTwoCharsAndThenPeekOneChar_PeekDoesNotAdvanceStream()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			await bufferedStreamReader.Peek(2);
			var peekedChar = await bufferedStreamReader.Peek(1);

			Assert.That(peekedChar, Is.EqualTo(new [] { 'a' }));
		}

		[Test]
		public async Task PeekTwoCharsAndThenPeekTwoChars_PeekDoesNotAdvanceStream()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			await bufferedStreamReader.Peek(2);
			var peekedChars = await bufferedStreamReader.Peek(2);

			CollectionAssert.AreEqual(charArray, peekedChars);
		}

		[Test]
		public async Task PeekMoreCharsThanStreamHas_ReturnsOnlyAvailableChars()
		{
			var charArray = new[] { 'a' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			var peekedChars = await bufferedStreamReader.Peek(2);

			CollectionAssert.AreEqual(charArray, peekedChars);
		}

		[Test]
		public async Task ReadMoreCharsThanStreamHas_ReturnsOnlyAvailableChars()
		{
			var charArray = new[] { 'a' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			var readChar = await bufferedStreamReader.Read();
			var nullChar = await bufferedStreamReader.Read();

			Assert.Multiple(() =>
				{
					Assert.That(readChar, Is.EqualTo('a'));
					Assert.Null(nullChar);
				}
			);
		}

		[Test]
		public void PeekZeroChars_Throws()
		{
			var charArray = new[] { 'a' };
			var stream = createStreamReaderFrom(charArray);
			using var bufferedStreamReader = new BufferedCharacterStreamReader(stream);

			Assert.ThrowsAsync<InvalidOperationException>(() => bufferedStreamReader.Peek(0).AsTask());
		}

		private static CharacterStreamReader createStreamReaderFrom(IEnumerable<char> chars)
		{
			return new CharacterStreamReader(
				new YamlCharacterStream(new MemoryStream(chars.Select(_ => (byte) _).ToArray()))
			);
		}
	}
}