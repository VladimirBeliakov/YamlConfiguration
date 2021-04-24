using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IOneLineCommentParser
	{
		ValueTask Process(ICharacterStream charStream);
	}
}