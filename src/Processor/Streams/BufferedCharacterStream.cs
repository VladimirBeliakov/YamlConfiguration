using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class BufferedCharacterStream : ICharacterStream, IAsyncDisposable
	{
		private const int _queueMaxSize = 1024;

		private readonly CharacterStream _charStream;
		private readonly Queue<char> _buffer = new Queue<char>(_queueMaxSize);

		public BufferedCharacterStream(CharacterStream charStream)
		{
			_charStream = charStream;
		}

		public async ValueTask<char> Peek() => (await Peek(1).ConfigureAwait(false)).Single();

		public async ValueTask<IReadOnlyCollection<char>> Peek(int charCount)
		{
			if (charCount == 0 || charCount > _queueMaxSize)
				throw new InvalidOperationException($"{nameof(charCount)} can only be from 1 up to {_queueMaxSize}.");

			var bufferLength = _buffer.Count;

			if (bufferLength >= charCount)
				return _buffer.Take(charCount).ToList();

			var missingCharCount = charCount - bufferLength;

			for (var i = 0; i < missingCharCount; i++)
			{
				var charRead = await _charStream.Read().ConfigureAwait(false);

				if (charRead is null)
					break;

				_buffer.Enqueue(charRead.Value);
			}

			return _buffer.ToList();
		}

		public async ValueTask<char?> Read()
		{
			if (_buffer.Count > 0)
				return _buffer.Dequeue();

			var charRead = await _charStream.Read().ConfigureAwait(false);

			return charRead;
		}

		public ValueTask DisposeAsync() => _charStream.DisposeAsync();
	}
}