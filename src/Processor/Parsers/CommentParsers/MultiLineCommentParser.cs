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

		public async ValueTask Process(ICharacterStream charStream)
		{
			while (await _oneLineCommentParser.TryProcess(charStream).ConfigureAwait(false)) {}
		}
	}
}