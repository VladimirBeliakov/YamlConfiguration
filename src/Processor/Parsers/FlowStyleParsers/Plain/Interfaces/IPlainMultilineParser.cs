using System.Threading.Tasks;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal interface IPlainMultilineParser
	{
		ValueTask<PlainLineNode?> TryProcess(ICharacterStream charStream, uint indentLength, Context context);
	}
}

