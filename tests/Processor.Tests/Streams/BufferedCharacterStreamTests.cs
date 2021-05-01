using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using YamlConfiguration.Processor;

namespace Sandbox
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class BufferedCharacterStreamTests
	{
		[Test]
		public async Task PickOneChar_ReturnsCorrectChar()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			var peekedChar = await bufferedStream.Peek();

			Assert.That(peekedChar, Is.EqualTo('a'));
		}

		[Test]
		public async Task PeekTwoChars_ReturnsCorrectChars()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			var peekedChars = await bufferedStream.Peek(2);

			CollectionAssert.AreEqual(charArray, peekedChars);
		}

		[Test]
		public async Task ReadOneChar_ReturnsCorrectChar()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			var readChar = await bufferedStream.Read();

			Assert.That(readChar, Is.EqualTo('a'));
		}

		[Test]
		public async Task ReadTwoChars_ReturnsCorrectChars()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			var readCharOne = await bufferedStream.Read();
			var readCharTwo = await bufferedStream.Read();

			Assert.Multiple(() =>
				{
					Assert.That(readCharOne, Is.EqualTo('a'));
					Assert.That(readCharTwo, Is.EqualTo('b'));
				}
			);
		}

		[Test]
		public async Task PeekTwoCharsAndThenReadTwoChars_PeekDoesNotAdvanceStream()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			await bufferedStream.Peek(2);
			var readCharOne = await bufferedStream.Read();
			var readCharTwo = await bufferedStream.Read();

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
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			await bufferedStream.Peek();
			var readCharOne = await bufferedStream.Read();
			var readCharTwo = await bufferedStream.Read();

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
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			await bufferedStream.Peek(2);
			var peekedChar = await bufferedStream.Peek();

			Assert.That(peekedChar, Is.EqualTo('a'));
		}

		[Test]
		public async Task PeekTwoCharsAndThenPeekTwoChars_PeekDoesNotAdvanceStream()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			await bufferedStream.Peek(2);
			var peekedChars = await bufferedStream.Peek(2);

			CollectionAssert.AreEqual(charArray, peekedChars);
		}

		[Test]
		public async Task PeekMoreCharsThanStreamHas_ReturnsOnlyAvailableChars()
		{
			var charArray = new[] { 'a' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			var peekedChars = await bufferedStream.Peek(2);

			CollectionAssert.AreEqual(charArray, peekedChars);
		}

		[Test]
		public async Task ReadMoreCharsThanStreamHas_ReturnsOnlyAvailableChars()
		{
			var charArray = new[] { 'a' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			var readChar = await bufferedStream.Read();
			var nullChar = await bufferedStream.Read();

			Assert.Multiple(() =>
				{
					Assert.That(readChar, Is.EqualTo('a'));
					Assert.Null(nullChar);
				}
			);
		}

		[Test]
		public async Task PeekZeroChars_Throws()
		{
			var charArray = new[] { 'a' };
			var stream = createStreamFrom(charArray);
			await using var bufferedStream = new BufferedCharacterStream(stream);

			Assert.ThrowsAsync<InvalidOperationException>(() => bufferedStream.Peek(0).AsTask());
		}

		private static CharacterStream createStreamFrom(IEnumerable<char> chars)
		{
			return new CharacterStream(new MemoryStream(chars.Select(_ => (byte) _).ToArray()));
		}
	}
}