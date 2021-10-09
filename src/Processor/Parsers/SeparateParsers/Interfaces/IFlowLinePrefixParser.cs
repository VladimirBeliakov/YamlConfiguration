using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IFlowLinePrefixParser
	{
		ValueTask<bool> TryProcess(ICharacterStream charStream, uint indentLength);
	}
}