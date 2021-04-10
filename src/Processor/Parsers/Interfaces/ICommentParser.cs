using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ICommentParser
	{
		ValueTask Process(ICharacterStream charStream);
	}
}