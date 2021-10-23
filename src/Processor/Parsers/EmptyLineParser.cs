using System;
using System.Threading.Tasks;
using YamlConfiguration.Processor;

internal class EmptyLineParser : IEmptyLineParser
{
	public async ValueTask<bool> TryProcess(ICharacterStream charStream)
	{
		var readLine = await charStream.PeekLine().ConfigureAwait(false);

		if (String.IsNullOrEmpty(readLine))
			return false;

		if (readLine[0] == Characters.Tab)
			throw new InvalidYamlException("An empty line can't begin with a tab.");

		if (readLine[^1] != BasicStructures.Break)
			return false;

		foreach (var @char in readLine[..^1])
			if (!@char.IsWhiteSpace())
				return false;

		await charStream.ReadLine().ConfigureAwait(false);

		return true;
	}
}
