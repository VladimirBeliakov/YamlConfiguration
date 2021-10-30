using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IFoldedLinesParser
	{
		ValueTask<FoldedLinesResult?> Process(ICharacterStream charStream);
	}
}
