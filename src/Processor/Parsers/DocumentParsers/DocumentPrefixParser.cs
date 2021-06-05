using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DocumentPrefixParser : IDocumentPrefixParser
	{
		private readonly IOneLineCommentParser _oneLineCommentParser;

		public DocumentPrefixParser(IOneLineCommentParser oneLineCommentParser)
		{
			_oneLineCommentParser = oneLineCommentParser;
		}

		public async ValueTask Process(ICharacterStream charStream)
		{
			while (await _oneLineCommentParser.TryProcess(charStream).ConfigureAwait(false)) {}
		}
	}
}