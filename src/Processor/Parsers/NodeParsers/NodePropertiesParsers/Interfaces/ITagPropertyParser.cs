using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ITagPropertyParser
	{
		ValueTask<TagProperty?> Process(ICharacterStream charStream);
	}
}