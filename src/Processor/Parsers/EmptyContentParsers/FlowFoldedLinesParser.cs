using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class FlowFoldedLinesParser : IFlowFoldedLinesParser
	{
		private readonly ISeparateInLineParser _separateInLineParser;
		private readonly IFoldedLinesParser _foldedLinesParser;
		private readonly IFlowLinePrefixParser _flowLinePrefixParser;

		public FlowFoldedLinesParser(
			ISeparateInLineParser separateInLineParser,
			IFoldedLinesParser foldedLinesParser,
			IFlowLinePrefixParser flowLinePrefixParser
		)
		{
			_separateInLineParser = separateInLineParser;
			_foldedLinesParser = foldedLinesParser;
			_flowLinePrefixParser = flowLinePrefixParser;
		}

		public async ValueTask<FoldedLinesResult?> TryProcess(ICharacterStream charStream, uint indentLength)
		{
			// Not checking the result because a separate in line is optional here.
			await _separateInLineParser.TryProcess(charStream).ConfigureAwait(false);

			var foldedLinesResult = await _foldedLinesParser.Process(charStream).ConfigureAwait(false);

			if (foldedLinesResult is null)
				return null;

			var isFlowLinePrefix =
				await _flowLinePrefixParser.TryProcess(charStream, indentLength).ConfigureAwait(false);

			return isFlowLinePrefix
				? foldedLinesResult
				: throw new InvalidYamlException("Folded lines must be followed by a flow line prefix.");
		}
	}
}
