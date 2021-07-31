using System.Collections.Generic;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ICharacterStream
	{
		ValueTask<char?> Peek();

		ValueTask<IReadOnlyList<char>> Peek(int charCount);

		ValueTask<char?> Read();

		IAsyncEnumerable<char> Read(int charCount);

		ValueTask<string> ReadLine();
	}
}