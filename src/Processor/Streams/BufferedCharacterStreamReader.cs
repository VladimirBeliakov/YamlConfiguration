using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class BufferedCharacterStreamReader : IDisposable
	{
		private const int _queueMaxSize = 1024;

		private readonly CharacterStreamReader _streamReader;
		private readonly Queue<char> _buffer = new Queue<char>(_queueMaxSize);

		public BufferedCharacterStreamReader(CharacterStreamReader streamReader)
		{
			_streamReader = streamReader;
		}

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
				var byteRead = await _streamReader.Read().ConfigureAwait(false);

				if (byteRead is null)
					break;

				_buffer.Enqueue(byteRead.Value);
			}

			return _buffer.ToList();
		}

		public async ValueTask<char?> Read()
		{
			if (_buffer.Count > 0)
				return _buffer.Dequeue();

			var charRead = await _streamReader.Read().ConfigureAwait(false);

			return charRead;
		}

		public void Dispose()
		{
			_streamReader.Dispose();
		}
	}
}