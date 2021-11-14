using System.Threading.Tasks;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal interface IPlainInOneLineParser
	{
		ValueTask<PlainLineNode?> Process(ICharacterStream charStream, Context context, bool asOneLine = false);
	}
}
