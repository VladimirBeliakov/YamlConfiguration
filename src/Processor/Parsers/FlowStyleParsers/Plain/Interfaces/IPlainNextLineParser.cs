using System.Threading.Tasks;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal interface IPlainNextLineParser
	{
		ValueTask<string?> TryProcess(ICharacterStream charStream, Context context);
	}
}

