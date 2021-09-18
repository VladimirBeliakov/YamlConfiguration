using System.Threading.Tasks;

namespace YamlConfiguration.Processor.SeparateParsers
{
	internal interface ISeparateInLineParser
	{
		ValueTask<bool> TryProcess(ICharacterStream charStream);
	}
}