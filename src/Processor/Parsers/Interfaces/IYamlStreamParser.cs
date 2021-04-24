using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IYamlStreamParser
	{
		ValueTask<YamlStream?> Process(ICharacterStream charStream);
	}
}