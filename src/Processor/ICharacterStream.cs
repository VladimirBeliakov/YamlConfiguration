using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal interface ICharacterStream
	{
		ValueTask<char> Peek();

		ValueTask<char?> Read();

		bool IsDisposed { get; }
	}
}