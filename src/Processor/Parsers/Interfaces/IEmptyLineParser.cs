using System.Threading.Tasks;
using YamlConfiguration.Processor;

internal interface IEmptyLineParser
{
	ValueTask<bool> TryProcess(ICharacterStream charStream);
}
