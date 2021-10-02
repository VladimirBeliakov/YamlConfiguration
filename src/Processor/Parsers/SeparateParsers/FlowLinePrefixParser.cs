using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class FlowLinePrefixParser
	{
		private readonly ISeparateInLineParser _separateInLineParser;

		public FlowLinePrefixParser(ISeparateInLineParser separateInLineParser)
		{
			_separateInLineParser = separateInLineParser;
		}

		public async ValueTask<bool> TryProcess(ICharacterStream charStream, uint indentLength)
		{
			if (indentLength > Characters.CharGroupMaxLength)
				throw new InvalidYamlException(
					$"{nameof(indentLength)} must not exceed {Characters.CharGroupMaxLength}."
				);

			var peekedChars = await charStream.Peek(indentLength).ConfigureAwait(false);

			if (peekedChars.Count != indentLength || peekedChars.Any(c => c != Characters.Space))
				return false;

			await charStream.AdvanceBy(indentLength).ConfigureAwait(false);

			var (_, whiteSpaceCount) = await _separateInLineParser.Peek(charStream).ConfigureAwait(false);

			if (whiteSpaceCount > 0)
				await charStream.AdvanceBy(whiteSpaceCount).ConfigureAwait(false);

			return true;
		}
	}
}