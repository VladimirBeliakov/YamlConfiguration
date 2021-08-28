using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal static class Extensions
	{
		public static async ValueTask AdvanceBy(this ICharacterStream charStream, int charCount) =>
			await charStream.Read(charCount).ToListAsync();
	}
}