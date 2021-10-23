using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class FoldedLinesParser
	{
		private readonly IEmptyLineParser _emptyLineParser;

		public FoldedLinesParser(IEmptyLineParser emptyLineParser)
		{
			_emptyLineParser = emptyLineParser;
		}

		public async ValueTask<FoldedLineResult> Process(ICharacterStream charStream)
		{
			var possibleBreak = await charStream.Peek().ConfigureAwait(false);

			if (possibleBreak != BasicStructures.Break)
				return new(0);

			await charStream.AdvanceBy(1).ConfigureAwait(false);

			var emptyLineCount = 0U;

			while (await _emptyLineParser.TryProcess(charStream).ConfigureAwait(false))
				emptyLineCount++;

			return new(emptyLineCount, isBreakAsSpace: emptyLineCount == 0);
		}
	}
}
