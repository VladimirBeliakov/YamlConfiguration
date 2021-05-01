using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class CharacterStream : IAsyncDisposable
	{
		private readonly Stream _stream;
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();
		private readonly byte[] _buffer = new byte[4096];

		private int _currentBytePosition;
		private int _bufferLength;

		public bool IsDisposed { get; private set; }

		public CharacterStream(Stream stream)
		{
			_stream = stream;
		}

		public ValueTask DisposeAsync() => dispose();

		public async ValueTask<char?> Read() => (char?) await read().ConfigureAwait(false);

		private async ValueTask<byte?> read()
		{
			if (IsDisposed)
				return null;

			if (tryGetCurrentByteFromBuffer(out var currentByte))
				return currentByte;

			await fillBuffer().ConfigureAwait(false);

			if (_bufferLength == 0)
			{
				await dispose().ConfigureAwait(false);
				return null;
			}

			return getBufferCurrentByteAndAdvance();
		}

		private async ValueTask fillBuffer()
		{
			var bytesRead = await _stream.ReadAsync(_buffer, _cts.Token).ConfigureAwait(false);

			if (bytesRead == 0)
				await dispose().ConfigureAwait(false);

			_bufferLength = bytesRead;
		}

		private byte getCurrentByte() => _buffer[_currentBytePosition];

		private bool tryGetCurrentByteFromBuffer(out byte currentByte)
		{
			if (_bufferLength > 0)
			{
				currentByte = getBufferCurrentByteAndAdvance();

				return true;
			}

			currentByte = default;

			return false;
		}

		private byte getBufferCurrentByteAndAdvance()
		{
			var currentByte = getCurrentByte();

			var bufferLastCharPosition = _bufferLength - 1;
			if (_currentBytePosition == bufferLastCharPosition)
			{
				_currentBytePosition = 0;
				_bufferLength = 0;
			}
			else
			{
				_currentBytePosition++;
			}

			return currentByte;
		}

		private async ValueTask dispose()
		{
			_cts.Cancel();
			await _stream.DisposeAsync().ConfigureAwait(false);
			IsDisposed = true;
		}
	}
}