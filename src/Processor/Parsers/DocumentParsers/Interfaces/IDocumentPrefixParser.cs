using System.Text;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IDocumentPrefixParser
	{
		ValueTask<Encoding?> Process(ICharacterStream charStream);
	}
}