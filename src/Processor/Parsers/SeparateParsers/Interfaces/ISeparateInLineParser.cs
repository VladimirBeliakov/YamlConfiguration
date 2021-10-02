using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ISeparateInLineParser
	{
		ValueTask<ParsedSeparateInLineResult> Peek(ICharacterStream charStream);
	}
}