using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IOneDirectiveParser
	{
		ValueTask<IDirective?> Process(ICharacterStream charStream);
	}
}