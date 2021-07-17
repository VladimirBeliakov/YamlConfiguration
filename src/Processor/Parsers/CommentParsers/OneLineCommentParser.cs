using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class OneLineCommentParser : IOneLineCommentParser
	{
		private const int _commentCharLength = 1;
		private const int _allowedCommentLength = _commentCharLength + Characters.CommentTextMaxLength;

		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var chars = await charStream.Peek(_commentCharLength + Characters.CharGroupMaxLength).ConfigureAwait(false);

			// It's EOF
			if (chars.Count == 0)
				return false;

			var whiteCharsSkipped = 0;

			foreach (var @char in chars)
				if (@char == Characters.Tab || @char == Characters.Space)
				{
					whiteCharsSkipped++;
				}
				else
				{
					if (@char == Characters.Comment || @char == BasicStructures.Break)
						break;

					return false;
				}

			if (whiteCharsSkipped > Characters.CharGroupMaxLength)
				throw new InvalidYamlException(
					$"Too many white space characters in the comment line. " +
					$"Allowed is {Characters.CharGroupMaxLength}."
				);

			// It's EOF
			if (whiteCharsSkipped == chars.Count)
			{
				await charStream.ReadLine().ConfigureAwait(false);
				return true;
			}

			var firstNotWhiteChar = chars[whiteCharsSkipped];

			if (firstNotWhiteChar != Characters.Comment && firstNotWhiteChar != BasicStructures.Break)
				return false;

			var readLine = await charStream.ReadLine().ConfigureAwait(false);
			var commentLength = readLine.Length - whiteCharsSkipped;

			if (commentLength > _allowedCommentLength)
				throw new InvalidYamlException($"Too long comment. Allowed length is {_allowedCommentLength}.");

			return true;
		}
	}
}