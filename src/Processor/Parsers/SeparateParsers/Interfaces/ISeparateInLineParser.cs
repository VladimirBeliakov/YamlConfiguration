using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal record ParsedSeparateInLineResult(bool IsSeparateInLine, int WhiteSpaceCount);

	internal interface ISeparateInLineParser
	{
		ValueTask<ParsedSeparateInLineResult> Peek(ICharacterStream charStream);
	}
}