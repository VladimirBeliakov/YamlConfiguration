using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class OneLineCommentParser : IOneLineCommentParser
	{
		// TODO: Allow for the \n\r Windows type break.
		private static readonly char _breakChar = BasicStructures.Break;

		public async ValueTask Process(ICharacterStream charStream)
		{
			var peekedChar = await charStream.Peek().ConfigureAwait(false);

			if (peekedChar != Characters.Comment)
				return;

			char? charRead;
			do
			{
				charRead = await charStream.Read().ConfigureAwait(false);

			} while (charRead.HasValue && charRead != _breakChar);
		}
	}
}