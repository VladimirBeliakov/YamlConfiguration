using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class OneLineCommentParser : IOneLineCommentParser
	{
		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var peekedChar = await charStream.Peek().ConfigureAwait(false);

			// TODO: Add skipping white spaces.
			if (peekedChar != Characters.Comment)
				return false;

			await charStream.ReadLine().ConfigureAwait(false);

			return true;
		}
	}
}