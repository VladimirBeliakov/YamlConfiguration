using System.Collections.Generic;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ICharacterStream
	{
		ValueTask<char?> Peek();

		ValueTask<IReadOnlyList<char>> Peek(uint charCount);

		ValueTask<string> PeekLine();

		ValueTask<char?> Read();

		IAsyncEnumerable<char> Read(uint charCount);

		ValueTask<string> ReadLine();

		bool IsAtStartOfLine { get; }
	}
}