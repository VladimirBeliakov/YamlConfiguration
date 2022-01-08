using System;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class EmptyLineParser : IEmptyLineParser
	{
		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var peekedLine = await charStream.PeekLine().ConfigureAwait(false);

			if (String.IsNullOrEmpty(peekedLine))
				return false;

			if (peekedLine[0] == Characters.Tab)
				throw new InvalidYamlException("An empty line can't begin with a tab.");

			if (peekedLine[^1] != BasicStructures.Break)
				return false;

			foreach (var @char in peekedLine[..^1])
				if (!@char.IsWhiteSpace())
					return false;

			await charStream.ReadLine().ConfigureAwait(false);

			return true;
		}
	}
}
