using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DocumentSuffixParser : IDocumentSuffixParser
	{
		private const char _dot = '.';
		private readonly ICommentParser _commentParser;

		public DocumentSuffixParser(ICommentParser commentParser)
		{
			_commentParser = commentParser;
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

			var isComment = await _commentParser.TryProcess(charStream).ConfigureAwait(false);

			if (!isComment)
				throw new InvalidYamlException("Only a comment may follow a document end.");

			await _commentParser.ProcessMultilineComments(charStream).ConfigureAwait(false);

			return true;
		}
	}
}