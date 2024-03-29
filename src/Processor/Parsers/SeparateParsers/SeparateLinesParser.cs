using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class SeparateLinesParser : ISeparateLinesParser
	{
		private readonly IMultilineCommentParser _multilineCommentParser;
		private readonly IFlowLinePrefixParser _flowLinePrefixParser;
		private readonly ISeparateInLineParser _separateInLineParser;

		public SeparateLinesParser(
			IMultilineCommentParser multilineCommentParser,
			IFlowLinePrefixParser flowLinePrefixParser,
			ISeparateInLineParser separateInLineParser
		)
		{
			_multilineCommentParser = multilineCommentParser;
			_flowLinePrefixParser = flowLinePrefixParser;
			_separateInLineParser = separateInLineParser;
		}

		public async ValueTask<bool> TryProcess(ICharacterStream charStream, uint? indentLength = null)
		{
			if (indentLength.HasValue)
			{
				var isComment =
					await _multilineCommentParser.TryProcess(charStream).ConfigureAwait(false);

				if (isComment)
					return await _flowLinePrefixParser.TryProcess(charStream, indentLength.Value)
						.ConfigureAwait(false);

				return false;
			}

			return await _separateInLineParser.TryProcess(charStream).ConfigureAwait(false);
		}
	}
}
