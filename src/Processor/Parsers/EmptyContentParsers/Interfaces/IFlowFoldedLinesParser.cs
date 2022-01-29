using System.Threading.Tasks;
using YamlConfiguration.Processor.FlowStyles;

namespace YamlConfiguration.Processor
{
	internal interface IFlowFoldedLinesParser
	{
		ValueTask<FlowFoldedLinesResult> Process(ICharacterStream charStream, uint indentLength);
	}
}
