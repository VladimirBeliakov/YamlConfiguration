using System.Collections.Generic;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IDirectiveParser
	{
		ValueTask<DirectiveParseResult> Process(ICharacterStream charStream);
	}
}