using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class YamlCharacterStreamTests
	{
		[TestCaseSource(nameof(getByteArraysWithExpectedEncodings))]
		public async Task DetectEncoding_StreamWithDifferentCharsAtBeginning_ReturnsExpectedEncoding(
			(byte[] byteArray, Encoding expectedEncoding) testCase
		)
		{
			var (byteArray, expectedEncoding) = testCase;
			await using var characterStream = createStreamFrom(byteArray);

			var encoding = await characterStream.DetectEncoding(CancellationToken.None);

			var errorMessage = $"Chars: {String.Join(", ", byteArray.Select(b => (char) b))}";
			Assert.That(encoding, Is.EqualTo(expectedEncoding), errorMessage);
		}

		[TestCaseSource(nameof(getByteArraysWithPreambleLengths))]
		public async Task Read_StreamWithPreamble_ReadsBytesSkippingPreamble(
			(byte[] byteArray, int preambleLength) testCase
		)
		{
			var (byteArray, preambleLength) = testCase;
			await using var characterStream = createStreamFrom(byteArray);
			await characterStream.DetectEncoding(CancellationToken.None);

			var buffer = new byte[byteArray.Length];
			var actualBytesReadCount = characterStream.Read(buffer, 0, buffer.Length);

			var expectedBytes = byteArray.Skip(preambleLength).ToArray();
			var expectedBytesReadCount = byteArray.Length - preambleLength;
			var errorMessage = $"Chars: {String.Join(", ", byteArray.Select(b => (char) b))}";
			Assert.Multiple(() =>
				{
					for (var i = 0; i < expectedBytes.Length; i++)
						Assert.That(buffer[i], Is.EqualTo(expectedBytes[i]), errorMessage);

					Assert.That(actualBytesReadCount, Is.EqualTo(expectedBytesReadCount), errorMessage);
				}
			);
		}

		[TestCase(1)]
		[TestCase(2)]
		public async Task Read_NotZeroOffsetAndNoPreambleInStream_BufferFilledFromOffset(int offset)
		{
			var byteArray = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
			await using var characterStream = createStreamFrom(byteArray);
			await characterStream.DetectEncoding(CancellationToken.None);

			var buffer = new byte[1024];
			characterStream.Read(buffer, offset, byteArray.Length);

			Assert.Multiple(() =>
				{
					for (var i = 0; i < offset; i++)
						Assert.That(buffer[i], Is.EqualTo(0x00));

					for (var i = 0; i < byteArray.Length; i++)
						Assert.That(buffer[i + offset], Is.EqualTo(byteArray[i]));
				}
			);
		}

		[TestCase(1)]
		[TestCase(2)]
		public async Task Read_NotZeroOffsetAndPreambleInStream_BufferFilledFromOffset(int offset)
		{
			var preamble = Encoding.UTF8.Preamble.ToArray();
			var byteArray = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
			var byteArrayWithPreamble = preamble.Concat(byteArray).ToArray();
			await using var characterStream = createStreamFrom(byteArrayWithPreamble);
			await characterStream.DetectEncoding(CancellationToken.None);

			var buffer = new byte[byteArray.Length + offset];
			characterStream.Read(buffer, offset, byteArray.Length);

			Assert.Multiple(() =>
				{
					for (var i = 0; i < offset; i++)
						Assert.That(buffer[i], Is.EqualTo(0x00));

					for (var i = 0; i < byteArray.Length; i++)
						Assert.That(buffer[i + offset], Is.EqualTo(byteArray[i]));
				}
			);
		}

		[Test]
		public async Task Read_DetectEncodingNotCalledBeforehand_Throws()
		{
			await using var characterStream = createStreamFrom(Array.Empty<byte>());

			Assert.Throws<InvalidOperationException>(() => characterStream.Read(new byte[1], 0, 0));
		}

		[Test]
		public async Task Read_NegativeOffset_Throws()
		{
			await using var characterStream = createStreamFrom(Array.Empty<byte>());
			await characterStream.DetectEncoding(CancellationToken.None);

			Assert.Throws<ArgumentOutOfRangeException>(() => characterStream.Read(new byte[1], -1, 0));
		}

		[Test]
		public async Task Read_NegativeCount_Throws()
		{
			await using var characterStream = createStreamFrom(Array.Empty<byte>());
			await characterStream.DetectEncoding(CancellationToken.None);

			Assert.Throws<ArgumentOutOfRangeException>(() => characterStream.Read(new byte[1], 0, -1));
		}

		[Test]
		public async Task Read_EmptyBuffer_Throws()
		{
			await using var characterStream = createStreamFrom(Array.Empty<byte>());
			await characterStream.DetectEncoding(CancellationToken.None);

			Assert.Throws<ArgumentException>(() => characterStream.Read(Array.Empty<byte>(), 0, 0));
		}

		[TestCase(1, 1, 1)]
		[TestCase(1, 0, 2)]
		public async Task Read_CountGreaterThanBufferLengthMinusOffset_Throws(int bufferLength, int offset, int count)
		{
			await using var characterStream = createStreamFrom(Array.Empty<byte>());
			await characterStream.DetectEncoding(CancellationToken.None);

			Assert.Throws<ArgumentException>(() => characterStream.Read(new byte[bufferLength], offset, count));
		}

		private static IEnumerable<(byte[], Encoding)> getByteArraysWithExpectedEncodings() =>
			getIntArraysWithExpectedEncodings().Select(a => (a.Item1.Select(i => (byte) i).ToArray(), a.Item2));

		private static IEnumerable<(int[], Encoding)> getIntArraysWithExpectedEncodings()
		{
			const int nullByte = 0x00;

			yield return (Array.Empty<int>(), Encodings.UTF8);
			yield return (new [] { nullByte }, Encodings.UTF8);
			yield return (new [] { 0xEF, 0xBB, 0xBF }, Encodings.UTF8);
			yield return (new [] { 0x01, 0x02, 0x03, 0x04 }, Encodings.UTF8);

			yield return (new [] { 0xFF, 0xFE, nullByte, nullByte }, Encodings.LittleEndianUTF32);
			foreach(var asciiChar in _asciiCharsExceptNullByte)
				yield return (new [] { asciiChar, nullByte, nullByte, nullByte }, Encodings.LittleEndianUTF32);

			yield return (new [] { nullByte, nullByte, 0xFE, 0xFF }, Encodings.BigEndianUTF32);
			foreach(var asciiChar in _asciiCharsExceptNullByte)
				yield return (new [] { nullByte, nullByte, nullByte, asciiChar }, Encodings.BigEndianUTF32);

			// When all bytes are null, the encoding definition order takes precedence
			// which means UTF32BE must be defined.
			yield return (new [] { nullByte, nullByte, nullByte, nullByte }, Encodings.BigEndianUTF32);

			yield return (new [] { 0xFE, 0xFF }, Encodings.BigEndianUTF16);
			foreach(var asciiChar in _asciiCharsExceptNullByte)
				yield return (new [] { nullByte, asciiChar }, Encodings.BigEndianUTF16);

			// When all bytes are null, the encoding definition order takes precedence
			// which means UTF16BE must be defined.
			yield return (new [] { nullByte, nullByte }, Encodings.BigEndianUTF16);

			foreach(var asciiChar in _asciiCharsExceptNullByte)
				yield return (new [] { asciiChar, nullByte }, Encodings.LittleEndianUTF16);
			yield return (new [] { 0xFF, 0xFE, nullByte }, Encodings.LittleEndianUTF16);
			yield return (new [] { 0xFF, 0xFE, 0x01, nullByte }, Encodings.LittleEndianUTF16);
			yield return (new [] { 0xFF, 0xFE, nullByte, 0x01 }, Encodings.LittleEndianUTF16);
		}

		private static IEnumerable<(byte[], int)> getByteArraysWithPreambleLengths() =>
			getIntArraysWithPreambleLengths().Select(a => (a.Item1.Select(i => (byte) i).ToArray(), a.Item2));

		private static IEnumerable<(int[], int)> getIntArraysWithPreambleLengths()
		{
			const byte asciiByte = 0x61;
			const byte someByte = 0x10;

			yield return (new [] { 0xFF, 0xFE, 0x00, 0x00, someByte }, 4);
			yield return (new [] { 0x00, 0x00, 0xFE, 0xFF, someByte }, 4);

			yield return (new [] { asciiByte, 0x00, 0x00, 0x00, someByte }, 0);
			yield return (new [] { 0x00, 0x00, 0x00, asciiByte, someByte }, 0);

			yield return (new [] { 0xEF, 0xBB, 0xBF, someByte }, 3);

			yield return (new [] { 0xFE, 0xFF, someByte }, 2);
			yield return (new [] { 0xFF, 0xFE, someByte }, 2);

			yield return (new [] { asciiByte, 0x00, someByte }, 0);
			yield return (new [] { 0x01, 0x02, 0x03, 0x04 }, 0);
			yield return (new [] { 0x01 }, 0);
		}

		private static YamlCharacterStream createStreamFrom(byte[] bytes) =>
			new YamlCharacterStream(new MemoryStream(bytes));

		private static readonly IEnumerable<char> _asciiCharsExceptNullByte =
			Enumerable.Range(1, SByte.MaxValue).Select(i => (char) i);
	}
}