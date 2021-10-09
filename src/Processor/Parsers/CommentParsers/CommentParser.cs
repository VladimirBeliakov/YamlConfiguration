using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class CommentParser : ICommentParser
	{
		private const int _commentCharLength = 1;
		private const int _allowedCommentLength = _commentCharLength + Characters.CommentTextMaxLength;

		private readonly ISeparateInLineParser _separateInLineParser;

		public CommentParser(ISeparateInLineParser separateInLineParser)
		{
			_separateInLineParser = separateInLineParser;
		}

		public async ValueTask<bool> TryProcess(ICharacterStream charStream, bool isLineComment = false)
		{
			var (isSeparateInLine, whiteSpaceCount) =
				await _separateInLineParser.Peek(charStream).ConfigureAwait(false);

			if (!isSeparateInLine && isLineComment)
				return false;

			var peekedChars = await charStream.Peek(whiteSpaceCount + 1).ConfigureAwait(false);
			var possibleCommentOrBreakChar = peekedChars.Count > whiteSpaceCount ? peekedChars[^1] : (char?) null;

			if (possibleCommentOrBreakChar is null)
			{
				if (whiteSpaceCount > 0)
					await charStream.AdvanceBy(whiteSpaceCount).ConfigureAwait(false);

				return true;
			}

			if (possibleCommentOrBreakChar == BasicStructures.Break)
			{
				await charStream.AdvanceBy(whiteSpaceCount + 1).ConfigureAwait(false);

				return true;
			}

			if (isSeparateInLine && possibleCommentOrBreakChar == Characters.Comment)
			{
				var readLine = await charStream.ReadLine().ConfigureAwait(false);

				var commentLength = readLine.Length - whiteSpaceCount;

				if (commentLength > _allowedCommentLength)
					throw new InvalidYamlException($"Too long comment. Allowed {_allowedCommentLength} chars.");

				return true;
			}

			return false;
		}
	}
}