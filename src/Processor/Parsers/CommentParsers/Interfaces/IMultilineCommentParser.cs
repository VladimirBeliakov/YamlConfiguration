using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IMultilineCommentParser
	{
		ValueTask<bool> TryProcess(ICharacterStream charStream);
	}
}