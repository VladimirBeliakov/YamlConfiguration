using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	public class CharacterStream : IAsyncEnumerable<char>
	{
		private readonly Stream _stream;

		public CharacterStream(Stream stream)
		{
			_stream = stream;
		}

		public IAsyncEnumerator<char> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			return new StreamEnumerator(_stream, cancellationToken);
		}

		private class StreamEnumerator : IAsyncEnumerator<char>
		{
			private readonly CancellationTokenSource _cts;
			private readonly Stream _stream;
			private readonly byte[] _buffer = new byte[4096];

			private int _currentBufferBytePosition;
			private int _bufferLength;

			private byte _currentByte;

			public char Current => (char) _currentByte;

			public StreamEnumerator(Stream stream, CancellationToken token)
			{
				_stream = stream;
				_cts = CancellationTokenSource.CreateLinkedTokenSource(token);
			}

			public async ValueTask<bool> MoveNextAsync()
			{
				if (tryGetBufferCurrentByte(out var currentByte))
				{
					_currentByte = currentByte;
					return true;
				}

				var bytesRead = await _stream.ReadAsync(_buffer, _cts.Token);

				if (bytesRead == 0)
					return false;

				_bufferLength = bytesRead;

				_currentByte = getBufferCurrentByte();

				return true;
			}

			public ValueTask DisposeAsync()
			{
				_cts.Cancel();
				_stream.Dispose();

				return ValueTask.CompletedTask;
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
		}
	}
}