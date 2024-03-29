using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class BufferedCharacterStreamReader : IDisposable
	{
		private const int _queueMaxSize = 1024 * 1024;

		private readonly EnsureBreakAtEofCharacterStreamReader _streamReader;
		private readonly Queue<char> _buffer = new(_queueMaxSize);

		public BufferedCharacterStreamReader(EnsureBreakAtEofCharacterStreamReader streamReader)
		{
			_streamReader = streamReader;
		}

		public async ValueTask<IReadOnlyCollection<char>> Peek(int charCount)
		{
			if (charCount is 0 or > _queueMaxSize)
				throw new InvalidOperationException($"{nameof(charCount)} can only be from 1 up to {_queueMaxSize}.");

			var bufferLength = _buffer.Count;

			if (bufferLength >= charCount)
				return _buffer.Take(charCount).ToList();

			var missingCharCount = charCount - bufferLength;

			for (var i = 0; i < missingCharCount; i++)
			{
				var charRead = await _streamReader.Read().ConfigureAwait(false);

				if (charRead is null)
					break;

				_buffer.Enqueue(charRead.Value);
			}

			return _buffer.ToList();
		}

		public async ValueTask<string> PeekLine()
		{
			var @break = BasicStructures.Break;

			var sb = new StringBuilder(_queueMaxSize);

			foreach (var @char in _buffer)
			{
				sb.Append(@char);

				if (@char == @break)
					return sb.ToString();
			}

			while (true)
			{
				var charRead = await _streamReader.Read().ConfigureAwait(false);

				if (charRead is null)
					break;

				if (_buffer.Count == _queueMaxSize)
					throw new InvalidOperationException($"Can't peek over {_queueMaxSize} chars.");

				_buffer.Enqueue(charRead.Value);

				sb.Append(charRead.Value);

				if (charRead == @break)
					break;
			}

			return sb.ToString();
		}

		public async ValueTask<char?> Read()
		{
			if (_buffer.Count > 0)
				return _buffer.Dequeue();

			var charRead = await _streamReader.Read().ConfigureAwait(false);

			return charRead;
		}

		public async ValueTask<string> ReadLine()
		{
			var @break = BasicStructures.Break;
			var sb = new StringBuilder();

			while (_buffer.Count > 0)
			{
				var @char = _buffer.Dequeue();

				sb.Append(@char);

				if (@char == @break)
					return sb.ToString();
			}

			while (true)
			{
				var charRead = await _streamReader.Read().ConfigureAwait(false);

				if (charRead is null)
					break;

				sb.Append(charRead.Value);

				if (charRead == @break)
					break;
			}

			return sb.ToString();
		}

		public void Dispose() => _streamReader.Dispose();
	}
}
