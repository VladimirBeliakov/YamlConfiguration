using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface INodeParser
	{
		ValueTask<IReadOnlyCollection<INode>> Process(ICharacterStream charStream);
	}
}