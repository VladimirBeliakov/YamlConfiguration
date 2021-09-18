using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DocumentPrefixParser : IDocumentPrefixParser
	{
		private readonly IMultiLineCommentParser _multiLineCommentParser;

		public DocumentPrefixParser(IMultiLineCommentParser multiLineCommentParser)
		{
			_multiLineCommentParser = multiLineCommentParser;
		}

		public async ValueTask Process(ICharacterStream charStream)
		{
			await _multiLineCommentParser.TryProcess(charStream).ConfigureAwait(false);
		}
	}
}