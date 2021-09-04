using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class TrackStartOfLineCharacterStreamReader : IDisposable
	{
		private readonly BufferedCharacterStreamReader _streamReader;

		public TrackStartOfLineCharacterStreamReader(BufferedCharacterStreamReader streamReader)
		{
			_streamReader = streamReader;
		}
		
		public bool IsAtStartOfLine { get; private set; } = true;

		public ValueTask<IReadOnlyCollection<char>> Peek(int charCount) => _streamReader.Peek(charCount);

		public ValueTask<string> PeekLine() => _streamReader.PeekLine();

		public async ValueTask<char?> Read()
		{
			var charRead = await _streamReader.Read();

			IsAtStartOfLine = charRead == BasicStructures.Break;

			return charRead;
		}

		public ValueTask<string> ReadLine()
		{
			IsAtStartOfLine = true;
			return _streamReader.ReadLine();
		}

		public void Dispose() => _streamReader.Dispose();
	}
}