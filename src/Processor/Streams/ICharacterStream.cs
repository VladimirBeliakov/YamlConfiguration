using System.Collections.Generic;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ICharacterStream
	{
		ValueTask<char> Peek();

		ValueTask<IReadOnlyCollection<char>> Peek(int charCount);

		ValueTask<char?> Read();

		ValueTask<string> ReadLine();
	}
}