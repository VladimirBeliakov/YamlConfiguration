using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	public class CharacterStream : IAsyncDisposable
	{
		private readonly Stream _stream;
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();
		private readonly byte[] _buffer = new byte[4096];

		private int _currentBufferBytePosition;
		private int _bufferLength;
		private bool _isDisposed;

		public CharacterStream(Stream stream)
		{
			_stream = stream;
		}

		public ValueTask DisposeAsync()
		{
			dispose();
			return ValueTask.CompletedTask;
		}

		public async ValueTask<char?> Read() => (char?) await read();

		private async ValueTask<byte?> read()
		{
			if (_isDisposed)
				throw new InvalidOperationException("Can't read a disposed stream.");

			if (tryGetBufferCurrentByte(out var currentByte))
				return currentByte;

			var bytesRead = await _stream.ReadAsync(_buffer, _cts.Token);

			if (bytesRead == 0)
			{
				dispose();
				return null;
			}

			_bufferLength = bytesRead;

			return getBufferCurrentByte();
		}

		private bool tryGetBufferCurrentByte(out byte currentByte)
		{
			if (_bufferLength > 0 && _currentBufferBytePosition <= getBufferLastItemPosition())
			{
				currentByte = getBufferCurrentByte();

				return true;
			}

			_currentBufferBytePosition = 0;
			_bufferLength = 0;

			currentByte = default;

			return false;
		}

		private byte getBufferCurrentByte()
		{
			if (_currentBufferBytePosition > getBufferLastItemPosition())
				throw new InvalidOperationException("Can't read over the size of the buffer.");

			var currentByte = _buffer[_currentBufferBytePosition];

			_currentBufferBytePosition++;

			return currentByte;
		}

		private int getBufferLastItemPosition() => _bufferLength - 1;

		private void dispose()
		{
			_cts.Cancel();
			_stream.Dispose();
			_isDisposed = true;
		}
	}
}