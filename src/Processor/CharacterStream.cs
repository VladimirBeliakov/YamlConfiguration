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

		private int _currentBufferBytePosition;
		private int _bufferLength;

		public bool IsDisposed { get; private set; }

		public CharacterStream(Stream stream)
		{
			_stream = stream;
		}

		public ValueTask DisposeAsync() => dispose();

		public async ValueTask<char> Peek()
		{
			if (IsDisposed)
				throw new InvalidOperationException("Can't peek a disposed stream.");

			if (_bufferLength == 0)
				await fillBuffer().ConfigureAwait(false);

			return (char) getCurrentByte();
		}

		public async ValueTask<char?> Read() => (char?) await read().ConfigureAwait(false);

		private async ValueTask<byte?> read()
		{
			if (IsDisposed)
				throw new InvalidOperationException("Can't read a disposed stream.");

			if (tryGetBufferCurrentByte(out var currentByte))
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

		private byte getCurrentByte() => _buffer[_currentBufferBytePosition];

		private bool tryGetBufferCurrentByte(out byte currentByte)
		{
			if (_bufferLength > 0 && _currentBufferBytePosition <= getBufferLastItemPosition())
			{
				currentByte = getBufferCurrentByteAndAdvance();

				return true;
			}

			_currentBufferBytePosition = 0;
			_bufferLength = 0;

			currentByte = default;

			return false;
		}

		private byte getBufferCurrentByteAndAdvance()
		{
			if (_currentBufferBytePosition > getBufferLastItemPosition())
				throw new InvalidOperationException("Can't read over the size of the buffer.");

			var currentByte = getCurrentByte();

			_currentBufferBytePosition++;

			return currentByte;
		}

		private int getBufferLastItemPosition() => _bufferLength - 1;

		private async ValueTask dispose()
		{
			_cts.Cancel();
			await _stream.DisposeAsync().ConfigureAwait(false);
			IsDisposed = true;
		}
	}
}