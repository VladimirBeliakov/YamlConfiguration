using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DocumentSuffixParser : IDocumentSuffixParser
	{
		private const char _dot = '.';
		private readonly IMultiLineCommentParser _multiLineCommentParser;

		public DocumentSuffixParser(IMultiLineCommentParser multiLineCommentParser)
		{
			_multiLineCommentParser = multiLineCommentParser;
		}

		public async ValueTask<bool> Process(ICharacterStream charStream)
		{
			var possibleDocumentEndChars = await charStream.Peek(3);

			if (possibleDocumentEndChars.Count < 3)
				return false;

			if (
				possibleDocumentEndChars[0] is not _dot ||
				possibleDocumentEndChars[1] is not _dot ||
				possibleDocumentEndChars[2] is not _dot
			)
				return false;

			// We need to advance the stream by three chars so then we can process any comments.
			await charStream.AdvanceBy(3).ConfigureAwait(false);

			var isComment = await _multiLineCommentParser.TryProcess(charStream).ConfigureAwait(false);

			if (!isComment)
				throw new InvalidYamlException("Only a comment may follow a document end.");

			return true;
		}
	}
}