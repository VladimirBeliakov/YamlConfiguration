using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ICommentParser
	{
		ValueTask<bool> TryProcess(ICharacterStream charStream);
	}
}