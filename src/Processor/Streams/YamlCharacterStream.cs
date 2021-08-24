using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class YamlCharacterStream : Stream
	{
		private const int _preambleMaxSize = 4;

		private readonly Stream _stream;
		private readonly Queue<byte> _buffer = new(_preambleMaxSize);
		private Encoding? _encoding;

		public YamlCharacterStream(Stream stream)
		{
			_stream = stream;
		}

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => _stream.Length;

		public async ValueTask<Encoding> DetectEncoding(CancellationToken token) =>
			_encoding ??= await detectEncodingInternal(token).ConfigureAwait(false);

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_encoding is null)
				throw new InvalidOperationException($"Call '{nameof(DetectEncoding)}' before reading bytes.");
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset), $"Passed value - ${offset}.");
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), $"Passed value - ${count}.");
			if (buffer.Length == 0)
				throw new ArgumentException($"{nameof(buffer)} must not be empty.");
			if (count > buffer.Length - offset)
				throw new ArgumentException(
					$"{nameof(count)} can't be greater than the length of '{nameof(buffer)}' minus '{nameof(offset)}'."
				);

			var bytesRead = 0;
			var currentReadingPosition = offset;

			while (_buffer.Count > 0)
			{
				buffer[currentReadingPosition++] = _buffer.Dequeue();

				bytesRead++;

				if (bytesRead == count)
					break;
			}

			var leftUnreadByteCount = count - bytesRead;

			if (leftUnreadByteCount > 0)
				bytesRead += _stream.Read(buffer, offset + bytesRead, leftUnreadByteCount);

			return bytesRead;
		}

		private async ValueTask<Encoding> detectEncodingInternal(CancellationToken token)
		{
			if (_encoding is not null)
				return _encoding;

			Encoding encoding;

			var preambleBytes = new byte[_preambleMaxSize];

			var bytesRead =
				await _stream.ReadAsync(preambleBytes, 0, preambleBytes.Length, token).ConfigureAwait(false);

			var firstByte = preambleBytes[0];
			var secondByte = preambleBytes[1];
			var thirdByte = preambleBytes[2];
			var forthByte = preambleBytes[3];
			var preambleLength = 0;

			if (bytesRead < 2)
			{
				encoding = Encodings.UTF8;
			}
			else if (
				bytesRead == 4 &&
				firstByte == 0x00 &&
				secondByte == 0x00 &&
				thirdByte == 0xFE &&
				forthByte == 0xFF
			)
			{
				encoding = Encodings.BigEndianUTF32;
				preambleLength = 4;
			}
			else if (
				bytesRead == 4 &&
				firstByte == 0x00 &&
				secondByte == 0x00 &&
				thirdByte == 0x00 &&
				isASCIIChar(forthByte)
			)
			{
				encoding = Encodings.BigEndianUTF32;
			}
			else if (
				bytesRead == 4 &&
				firstByte == 0xFF &&
				secondByte == 0xFE &&
				thirdByte == 0x00 &&
				forthByte == 0x00
			)
			{
				encoding = Encodings.LittleEndianUTF32;
				preambleLength = 4;
			}
			else if (
				bytesRead == 4 &&
				isASCIIChar(firstByte) &&
				secondByte == 0x00 &&
				thirdByte == 0x00 &&
				forthByte == 0x00
			)
			{
				encoding = Encodings.LittleEndianUTF32;
			}
			else if (firstByte == 0xFE && secondByte == 0xFF)
			{
				encoding = Encodings.BigEndianUTF16;
				preambleLength = 2;
			}
			else if (firstByte == 0x00 && isASCIIChar(secondByte))
			{
				encoding = Encodings.BigEndianUTF16;
			}
			else if (firstByte == 0xFF && secondByte == 0xFE)
			{
				encoding = Encodings.LittleEndianUTF16;
				preambleLength = 2;
			}
			else if (isASCIIChar(firstByte) && secondByte == 0x00)
			{
				encoding = Encodings.LittleEndianUTF16;
			}
			else if (
				bytesRead >= 3 &&
				firstByte == 0xEF &&
				secondByte == 0xBB &&
				thirdByte == 0xBF
			)
			{
				encoding = Encodings.UTF8;
				preambleLength = 3;
			}
			else
			{
				encoding = Encodings.UTF8;
			}

			if (bytesRead > 0)
				copyToBufferFrom(preambleBytes.Take(bytesRead).ToArray(), preambleLength);

			return encoding;
		}

		public override ValueTask DisposeAsync() => _stream.DisposeAsync();

		public override void Flush() => _stream.Flush();

		private void copyToBufferFrom(byte[] bytes, int itemSkipCount)
		{
			if (bytes.Length == 0)
				throw new ArgumentException($"{nameof(bytes)} must contain at least one byte.");
			if (itemSkipCount > bytes.Length)
				throw new ArgumentException($"{nameof(itemSkipCount)} must not be greater than {nameof(bytes)}' count.");
			if (itemSkipCount < 0)
				throw new ArgumentOutOfRangeException(nameof(itemSkipCount), "Can't be negative.");

			for (var i = itemSkipCount; i < bytes.Length; i++)
				_buffer.Enqueue(bytes[i]);
		}

		private static bool isASCIIChar(byte value) => value <= SByte.MaxValue;

		#region Not Implemented Section

		public override long Position
		{
			get => throw new NotSupportedException("Seeking not supported.");
			set => throw new NotSupportedException("Seeking not supported.");
		}

		public override long Seek(long offset, SeekOrigin origin) =>
			throw new NotSupportedException("Seeking not supported.");

		public override void SetLength(long value) => throw new NotSupportedException("Seeking not supported.");

		public override void Write(byte[] buffer, int offset, int count) =>
			throw new NotSupportedException("Seeking not supported.");

		#endregion
	}
}