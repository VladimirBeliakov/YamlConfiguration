using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal static class CharacterStreamExtensions
	{
		public static async ValueTask AdvanceBy(this ICharacterStream charStream, int charCount) =>
			await charStream.Read(charCount).ToListAsync().ConfigureAwait(false);
	}
}