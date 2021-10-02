using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal static class CharacterStreamExtensions
	{
		public static async ValueTask AdvanceBy(this ICharacterStream charStream, uint charCount)
		{
			if (charCount is 0)
				return;

			await charStream.Read(charCount).ToListAsync().ConfigureAwait(false);
		}
	}
}