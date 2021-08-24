using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class CharacterStreamReader : IDisposable
	{
		private readonly YamlCharacterStream _stream;
		private StreamReader? _innerStreamReader;

		private readonly CancellationTokenSource _cts = new();
		private readonly char[] _buffer = new char[4096];

		private int _currentCharPosition;
		private int _bufferLength;
		private bool _isDisposed;

		public CharacterStreamReader(YamlCharacterStream stream)
		{
			_stream = stream;
		}

		public void Dispose() => dispose();

		public async ValueTask<char?> Read()
		{
			await ensureStreamReaderInitialized().ConfigureAwait(false);

			if (_isDisposed)
				return null;

			if (tryGetCurrentCharFromBuffer(out var currentChar))
				return currentChar;

			await fillBuffer().ConfigureAwait(false);

			if (_bufferLength == 0)
			{
				dispose();
				return null;
			}

			return getBufferCurrentCharAndAdvance();
		}

		private async ValueTask ensureStreamReaderInitialized()
		{
			if (_innerStreamReader is not null)
				return;

			var streamEncoding = await _stream.DetectEncoding(_cts.Token).ConfigureAwait(false);

			_innerStreamReader =
				new StreamReader(_stream, streamEncoding, detectEncodingFromByteOrderMarks: false);
		}

		private async ValueTask fillBuffer()
		{
			var charsRead = await _innerStreamReader!.ReadAsync(_buffer, _cts.Token).ConfigureAwait(false);

			if (charsRead == 0)
				dispose();

			_bufferLength = charsRead;
		}

		private char getCurrentChar() => _buffer[_currentCharPosition];

		private bool tryGetCurrentCharFromBuffer(out char currentChar)
		{
			if (_bufferLength > 0)
			{
				currentChar = getBufferCurrentCharAndAdvance();

				return true;
			}

			currentChar = default;

			return false;
		}

		private char getBufferCurrentCharAndAdvance()
		{
			var currentChar = getCurrentChar();

			var bufferLastCharPosition = _bufferLength - 1;
			if (_currentCharPosition == bufferLastCharPosition)
			{
				_currentCharPosition = 0;
				_bufferLength = 0;
			}
			else
			{
				_currentCharPosition++;
			}

			return currentChar;
		}

		private void dispose()
		{
			_cts.Cancel();
			_innerStreamReader?.Dispose();
			_isDisposed = true;
		}
	}
}