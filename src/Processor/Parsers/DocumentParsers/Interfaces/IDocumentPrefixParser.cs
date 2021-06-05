using System.Text;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IDocumentPrefixParser
	{
		ValueTask Process(ICharacterStream charStream);
	}
}