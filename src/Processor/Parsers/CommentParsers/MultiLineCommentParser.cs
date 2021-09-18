using System;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class MultiLineCommentParser : IMultiLineCommentParser
	{
		private readonly IOneLineCommentParser _oneLineCommentParser;

		public MultiLineCommentParser(IOneLineCommentParser oneLineCommentParser)
		{
			_oneLineCommentParser = oneLineCommentParser;
		}

		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var isComment = await _oneLineCommentParser.TryProcess(charStream).ConfigureAwait(false);

			if (!isComment)
				return false;

			while (await _oneLineCommentParser.TryProcess(charStream).ConfigureAwait(false)) {}

			return true;
		}
	}
}