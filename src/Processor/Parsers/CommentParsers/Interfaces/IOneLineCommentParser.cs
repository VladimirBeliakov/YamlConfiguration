using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IOneLineCommentParser
	{
		ValueTask<bool> TryProcess(ICharacterStream charStream);
	}
}