using System.Threading.Tasks;
using YamlConfiguration.Processor.FlowStyles;

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

		public async ValueTask<FlowFoldedLinesResult> Process(ICharacterStream charStream, uint indentLength)
		{
			var (isSeparateInLine, whiteSpaceCount) =
				await _separateInLineParser.Peek(charStream).ConfigureAwait(false);

			if (isSeparateInLine)
				await charStream.AdvanceBy(whiteSpaceCount).ConfigureAwait(false);

			var foldedLinesResult =
				await _foldedLinesParser.Process(charStream).ConfigureAwait(false);

			var result = new FlowFoldedLinesResult(whiteSpaceCount, foldedLinesResult);

			if (foldedLinesResult is null)
				return result;

			var isFlowLinePrefix =
				await _flowLinePrefixParser.TryProcess(charStream, indentLength).ConfigureAwait(false);

			return isFlowLinePrefix
				? result
				: throw new InvalidYamlException("Folded lines must be followed by a flow line prefix.");
		}
	}
}
