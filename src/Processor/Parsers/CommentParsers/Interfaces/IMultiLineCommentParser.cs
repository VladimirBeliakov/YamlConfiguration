using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IMultiLineCommentParser
	{
		ValueTask Process(ICharacterStream charStream);
	}
}