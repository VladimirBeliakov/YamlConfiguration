using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class SeparateInLineParser : ISeparateInLineParser
	{
		public async ValueTask<ParsedSeparateInLineResult> Peek(ICharacterStream charStream)
		{
			var peekedChar = await charStream.Peek().ConfigureAwait(false);

			if (peekedChar?.IsWhiteSpace() is false)
				return new ParsedSeparateInLineResult(IsSeparateInLine: charStream.IsAtStartOfLine, WhiteSpaceCount: 0);

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

			return new ParsedSeparateInLineResult(IsSeparateInLine: true, WhiteSpaceCount: whiteSpaceCount);
		}
	}
}