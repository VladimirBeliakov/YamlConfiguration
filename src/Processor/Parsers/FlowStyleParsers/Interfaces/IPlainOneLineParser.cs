using System.Threading.Tasks;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal interface IPlainOneLineParser
	{
		ValueTask<PlainOneLineNode?> Process(ICharacterStream charStream, Context context);
	}
}