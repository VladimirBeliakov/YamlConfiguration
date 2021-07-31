using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DocumentSuffixParser : IDocumentSuffixParser
	{
		private const char _dot = '.';
		private readonly IOneLineCommentParser _oneLineCommentParser;
		private readonly IMultiLineCommentParser _multiLineCommentParser;

		public DocumentSuffixParser(
			IOneLineCommentParser oneLineCommentParser,
			IMultiLineCommentParser multiLineCommentParser
		)
		{
			_oneLineCommentParser = oneLineCommentParser;
			_multiLineCommentParser = multiLineCommentParser;
		}

		public async ValueTask<bool> Process(ICharacterStream charStream)
		{
			var possibleDocumentEndChars = await charStream.Peek(4);

			if (possibleDocumentEndChars.Count < 3)
				return false;

			if (
				possibleDocumentEndChars[0] is not _dot ||
				possibleDocumentEndChars[1] is not _dot ||
				possibleDocumentEndChars[2] is not _dot
			)
				return false;

			// It's EOF
			if (possibleDocumentEndChars.Count is 3)
			{
				await charStream.ReadLine().ConfigureAwait(false);
				return true;
			}

			if (
				possibleDocumentEndChars[3] != Characters.Space &&
				possibleDocumentEndChars[3] != Characters.Tab &&
				possibleDocumentEndChars[3] != BasicStructures.Break
			)
				return false;

			// We need to advance the stream by four chars so then we can process any comments.
			await charStream.Read(4).ToListAsync().ConfigureAwait(false);

			if (possibleDocumentEndChars[3] != BasicStructures.Break)
			{
				var isComment = await _oneLineCommentParser.TryProcess(charStream).ConfigureAwait(false);

				if (!isComment)
				{
					var invalidString = await charStream.ReadLine().ConfigureAwait(false);
					throw new InvalidYamlException(
						$"Only a comment may follow a document end. Actual string - '{invalidString}'."
					);
				}
			}

			await _multiLineCommentParser.Process(charStream).ConfigureAwait(false);

			return true;
		}
	}
}