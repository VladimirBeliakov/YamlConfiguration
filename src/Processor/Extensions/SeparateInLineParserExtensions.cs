using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal static class SeparateInLineParserExtensions
	{
		public static async ValueTask<bool> TryProcess(
			this ISeparateInLineParser separateInLineParser,
			ICharacterStream charStream
		)
		{
			var (isSeparateInLine, whiteSpaceCount) = await separateInLineParser.Peek(charStream).ConfigureAwait(false);

			if (isSeparateInLine)
			{
				await charStream.AdvanceBy(whiteSpaceCount).ConfigureAwait(false);
				return true;
			}

			return false;
		}
	}
}