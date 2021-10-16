using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ISeparateLinesParser
	{
		ValueTask<bool> TryProcess(ICharacterStream charStream, uint? indentLength = null);
	}
}