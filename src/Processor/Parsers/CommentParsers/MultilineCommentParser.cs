using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class MultilineCommentParser : IMultilineCommentParser
	{
		private readonly ICommentParser _commentParser;

		public MultilineCommentParser(ICommentParser commentParser) => _commentParser = commentParser;

		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			if (!charStream.IsAtStartOfLine)
			{
				var isComment = await _commentParser.TryProcess(charStream).ConfigureAwait(false);

				if (!isComment)
					return false;
			}

			await _commentParser.ProcessLineComments(charStream).ConfigureAwait(false);

			return true;
		}
	}
}