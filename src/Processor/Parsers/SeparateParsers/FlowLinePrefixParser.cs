using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor.SeparateParsers
{
	internal class FlowLinePrefixParser
	{
		private readonly ISeparateInLineParser _separateInLineParser;

		public FlowLinePrefixParser(ISeparateInLineParser separateInLineParser)
		{
			_separateInLineParser = separateInLineParser;
		}

		public async ValueTask<bool> TryProcess(ICharacterStream charStream, int indentLength)
		{
			if (indentLength > Characters.CharGroupMaxLength)
				throw new InvalidYamlException(
					$"{nameof(indentLength)} must not exceed {Characters.CharGroupMaxLength}."
				);

			var peekedChars = await charStream.Peek(indentLength).ConfigureAwait(false);

			if (peekedChars.Any(c => c != Characters.Space) || peekedChars.Count != indentLength)
				return false;

			await charStream.AdvanceBy(indentLength).ConfigureAwait(false);

			// Not checking the result because a separate in line may not exist.
			await _separateInLineParser.TryProcess(charStream).ConfigureAwait(false);

			return true;
		}
	}
}