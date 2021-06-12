using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IDocumentParser
	{
		ValueTask<Document?> Process(ICharacterStream charStream);
	}
}