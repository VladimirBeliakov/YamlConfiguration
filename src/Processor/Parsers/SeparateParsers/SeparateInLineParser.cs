using System.Threading.Tasks;
using YamlConfiguration.Processor.Extensions;

namespace YamlConfiguration.Processor.SeparateParsers
{
	internal class SeparateInLineParser
	{
		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var peekedChar = await charStream.Peek().ConfigureAwait(false);

			if (peekedChar?.IsWhiteSpace() is false)
				return charStream.IsAtStartOfLine;

			var peekedChars = await charStream.Peek(Characters.CharGroupMaxLength + 1).ConfigureAwait(false);

			var whiteSpaceCount = 0;
			foreach (var @char in peekedChars)
				if (@char.IsWhiteSpace())
					whiteSpaceCount++;
				else
					break;

			if (whiteSpaceCount > Characters.CharGroupMaxLength)
				throw new InvalidYamlException(
					$"Too many white space characters in the line. Allowed {Characters.CharGroupMaxLength} only."
				);

			await charStream.AdvanceBy(whiteSpaceCount).ConfigureAwait(false);

			return true;
		}
	}
}