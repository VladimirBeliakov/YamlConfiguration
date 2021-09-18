using System;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class MultiLineCommentParser : IMultiLineCommentParser
	{
		private readonly ICommentParser _commentParser;

		public MultiLineCommentParser(ICommentParser commentParser)
		{
			_commentParser = commentParser;
		}

		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var isComment = await _commentParser.TryProcess(charStream).ConfigureAwait(false);

			if (!isComment)
				return false;

			while (await _commentParser.TryProcess(charStream).ConfigureAwait(false)) {}

			return true;
		}
	}
}