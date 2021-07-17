using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class OneLineCommentParser : IOneLineCommentParser
	{
		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			char? peekedChar;
			var whiteCharCount = 0;
			while (true)
			{
				peekedChar = await charStream.Peek().ConfigureAwait(false);

				if (peekedChar != Characters.Space && peekedChar != Characters.Tab)
					break;

				if (whiteCharCount == Characters.CharGroupMaxLength)
					throw new InvalidYamlException(
						$"Too many white space characters in the comment line. " +
						$"Allowed is {Characters.CharGroupMaxLength}."
					);

				await charStream.Read().ConfigureAwait(false);

				whiteCharCount++;
			}

			if (peekedChar != Characters.Comment)
				return false;

			var readCommentLine = await charStream.ReadLine().ConfigureAwait(false);

			const int commentCharLength = 1;
			const int allowedCommentLength = commentCharLength + Characters.CommentTextMaxLength;

			if (readCommentLine.Length > allowedCommentLength)
				throw new InvalidYamlException($"Too long comment. Allowed length is {allowedCommentLength}.");

			return true;
		}
	}
}