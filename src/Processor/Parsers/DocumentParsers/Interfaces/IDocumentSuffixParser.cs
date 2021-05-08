using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IDocumentSuffixParser
	{
		ValueTask<bool> Process(ICharacterStream charStream);
	}
}