using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DocumentPrefixParser : IDocumentPrefixParser
	{
		private readonly ICommentParser _commentParser;

		public DocumentPrefixParser(ICommentParser commentParser)
		{
			_commentParser = commentParser;
		}

		public ValueTask Process(ICharacterStream charStream) => _commentParser.ProcessLineComments(charStream);
	}
}