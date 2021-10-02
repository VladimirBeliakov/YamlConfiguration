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

		public async ValueTask<bool> TryProcess(ICharacterStream charStream, int indentLength)
		{
			if (indentLength > Characters.CharGroupMaxLength)
				throw new InvalidYamlException(
					$"{nameof(indentLength)} must not exceed {Characters.CharGroupMaxLength}."
				);

			var peekedChars = await charStream.Peek(indentLength).ConfigureAwait(false);

			if (peekedChars.Count != indentLength || peekedChars.Any(c => c != Characters.Space))
				return false;

			await charStream.AdvanceBy(indentLength).ConfigureAwait(false);

			// Not checking the result because a separate in line may not exist.
			await _separateInLineParser.Peek(charStream).ConfigureAwait(false);

			return true;
		}
	}
}