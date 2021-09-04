using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class CharacterStreamReaderTests
	{
		[Test]
		public async Task Read_StreamInUTF8Encoding_ReturnsDecodedChars()
		{
			var dollar = new byte[] { 0x24 };
			var cent = new byte[] { 0xC2, 0xA2 };
			var euro = new byte[] { 0xE2, 0x82, 0xAC };
			var hwair = new byte[] { 0xF0, 0x90, 0x8D, 0x88 };
			var bytes = concatArrays(dollar, cent, euro, hwair);
			var stream = createStreamFrom(bytes, Encodings.UTF8);
			var streamReader = new CharacterStreamReader(stream);

			var chars = await read(streamReader);

			Assert.That(chars, Has.Count.EqualTo(5), $"Got only {string.Join(", ", chars)}.");
			Assert.Multiple(() =>
				{
					Assert.That(chars[0], Is.EqualTo('$'));
					Assert.That(chars[1], Is.EqualTo('¢'));
					Assert.That(chars[2], Is.EqualTo('€'));

					var actualHwair = new String(new[] { chars[3], chars[4] });
					Assert.That(actualHwair, Is.EqualTo("𐍈"));
				}
			);
		}

		[Test]
		public async Task Read_StreamInBigEndianUTF16Encoding_ReturnsDecodedChars()
		{
			var dollar = new byte[] { 0x00, 0x24 };
			var euro = new byte[] { 0x20, 0xAC };
			var deseretYee = new byte[] { 0xD8, 0x01, 0xDC, 0x37 };
			var deseretEw = new byte[] { 0xD8, 0x01, 0xDC, 0x4F };
			var bytes = concatArrays(dollar, euro, deseretYee, deseretEw);
			var stream = createStreamFrom(bytes, Encodings.BigEndianUTF16);
			var streamReader = new CharacterStreamReader(stream);

			var chars = await read(streamReader);

			Assert.That(chars, Has.Count.EqualTo(6), $"Got only {string.Join(", ", chars)}.");
			Assert.Multiple(() =>
				{
					Assert.That(chars[0], Is.EqualTo('$'));
					Assert.That(chars[1], Is.EqualTo('€'));

					var actualDeseretYee = new String(new[] { chars[2], chars[3] });
					Assert.That(actualDeseretYee, Is.EqualTo("𐐷"));

					var actualDeseretEw = new String(new[] { chars[4], chars[5] });
					Assert.That(actualDeseretEw, Is.EqualTo("𐑏"));
				}
			);
		}

		[Test]
		public async Task Read_StreamInLittleEndianUTF16Encoding_ReturnsDecodedChars()
		{
			var dollar = new byte[] { 0x24, 0x00 };
			var euro = new byte[] { 0xAC, 0x20 };
			var deseretYee = new byte[] { 0x01, 0xD8, 0x37, 0xDC };
			var deseretEw = new byte[] { 0x01, 0xD8, 0x4F, 0xDC };
			var bytes = concatArrays(dollar, euro, deseretYee, deseretEw);
			var stream = createStreamFrom(bytes, Encodings.LittleEndianUTF16);
			var streamReader = new CharacterStreamReader(stream);

			var chars = await read(streamReader);

			Assert.That(chars, Has.Count.EqualTo(6), $"Got only {string.Join(", ", chars)}.");
			Assert.Multiple(() =>
				{
					Assert.That(chars[0], Is.EqualTo('$'));
					Assert.That(chars[1], Is.EqualTo('€'));

					var actualDeseretYee = new String(new[] { chars[2], chars[3] });
					Assert.That(actualDeseretYee, Is.EqualTo("𐐷"));

					var actualDeseretEw = new String(new[] { chars[4], chars[5] });
					Assert.That(actualDeseretEw, Is.EqualTo("𐑏"));
				}
			);
		}

		[Test]
		public async Task Read_StreamInBigEndianUTF32Encoding_ReturnsDecodedChars()
		{
			var dollar = new byte[] { 0x00, 0x00, 0x00, 0x24 };
			var euro = new byte[] { 0x00, 0x00, 0x20, 0xAC };
			var deseretYee = new byte[] { 0x00, 0x01, 0x04, 0x37 };
			var deseretEw = new byte[] { 0x00, 0x01, 0x04, 0x4F };
			var bytes = concatArrays(dollar, euro, deseretYee, deseretEw);
			var stream = createStreamFrom(bytes, Encodings.BigEndianUTF32);
			var streamReader = new CharacterStreamReader(stream);

			var chars = await read(streamReader);

			Assert.That(chars, Has.Count.EqualTo(6), $"Got only {string.Join(", ", chars)}.");
			Assert.Multiple(() =>
				{
					Assert.That(chars[0], Is.EqualTo('$'));
					Assert.That(chars[1], Is.EqualTo('€'));

					var actualDeseretYee = new String(new[] { chars[2], chars[3] });
					Assert.That(actualDeseretYee, Is.EqualTo("𐐷"));

					var actualDeseretEw = new String(new[] { chars[4], chars[5] });
					Assert.That(actualDeseretEw, Is.EqualTo("𐑏"));
				}
			);
		}

		[Test]
		public async Task Read_StreamInLittleEndianUTF32Encoding_ReturnsDecodedChars()
		{
			var dollar = new byte[] { 0x24, 0x00, 0x00, 0x00 };
			var euro = new byte[] { 0xAC, 0x20, 0x00, 0x00 };
			var deseretYee = new byte[] { 0x37, 0x04, 0x01, 0x00 };
			var deseretEw = new byte[] { 0x4F, 0x04, 0x01, 0x00 };
			var bytes = concatArrays(dollar, euro, deseretYee, deseretEw);
			var stream = createStreamFrom(bytes, Encodings.LittleEndianUTF32);
			var streamReader = new CharacterStreamReader(stream);

			var chars = await read(streamReader);

			Assert.That(chars, Has.Count.EqualTo(6), $"Got only {string.Join(", ", chars)}.");
			Assert.Multiple(() =>
				{
					Assert.That(chars[0], Is.EqualTo('$'));
					Assert.That(chars[1], Is.EqualTo('€'));

					var actualDeseretYee = new String(new[] { chars[2], chars[3] });
					Assert.That(actualDeseretYee, Is.EqualTo("𐐷"));

					var actualDeseretEw = new String(new[] { chars[4], chars[5] });
					Assert.That(actualDeseretEw, Is.EqualTo("𐑏"));
				}
			);
		}

		private static IEnumerable<byte> concatArrays(params byte[][] arrays) => arrays.SelectMany(a => a);

		private static async ValueTask<IReadOnlyList<char>> read(CharacterStreamReader streamReader)
		{
			var chars = new List<char>();
			char? @char;
			while ((@char = await streamReader.Read()) != null)
				chars.Add(@char.Value);

			return chars;
		}

		private static YamlCharacterStream createStreamFrom(IEnumerable<byte> bytes, Encoding encoding)
		{
			var preamble = encoding.Preamble.ToArray();

			return new YamlCharacterStream(new MemoryStream(preamble.Concat(bytes).ToArray()));
		}
	}
}