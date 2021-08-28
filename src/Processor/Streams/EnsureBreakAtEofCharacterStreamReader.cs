using System;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class EnsureBreakAtEofCharacterStreamReader : IDisposable
	{
		private static readonly char _break = BasicStructures.Break;
		private readonly CharacterStreamReader _stream;
		private char _secondToLastChar;
		private bool _hasBreakAtEofBeenEnsured;

		public EnsureBreakAtEofCharacterStreamReader(CharacterStreamReader stream)
		{
			_stream = stream;
		}

		public async ValueTask<char?> Read()
		{
			var charRead = await _stream.Read().ConfigureAwait(false);

			if (charRead is null && _secondToLastChar != _break && !_hasBreakAtEofBeenEnsured)
			{
				_hasBreakAtEofBeenEnsured = true;
				return _break;
			}

			if (charRead.HasValue)
				_secondToLastChar = charRead.Value;

			return charRead;
		}

		public void Dispose()
		{
			_stream.Dispose();
		}
	}
}