using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IMultiLineCommentParser
	{
		ValueTask<bool> TryProcess(ICharacterStream charStream);
	}
}