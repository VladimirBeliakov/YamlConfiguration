using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IAliasNodeParser
	{
		ValueTask<INode?> Process(ICharacterStream charStream);
	}
}