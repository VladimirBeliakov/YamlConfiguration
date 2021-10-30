using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IFlowFoldedLinesParser
	{
		ValueTask<FoldedLinesResult?> TryProcess(ICharacterStream charStream, uint indentLength);
	}
}
