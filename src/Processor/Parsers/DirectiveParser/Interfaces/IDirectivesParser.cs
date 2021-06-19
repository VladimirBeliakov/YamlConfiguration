using System.Collections.Generic;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface IDirectivesParser
	{
		ValueTask<DirectiveParseResult> Process(ICharacterStream charStream);
	}
}