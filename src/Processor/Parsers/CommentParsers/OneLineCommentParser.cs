using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class OneLineCommentParser : IOneLineCommentParser
	{
		// TODO: Allow for the \n\r Windows type break.
		private static readonly char _breakChar = BasicStructures.Break;

		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var peekedChar = await charStream.Peek().ConfigureAwait(false);

			// TODO: Add skipping white spaces.
			if (peekedChar != Characters.Comment)
				return false;

			char? charRead;
			do
			{
				charRead = await charStream.Read().ConfigureAwait(false);

			} while (charRead.HasValue && charRead != _breakChar);

			return true;
		}
	}
}