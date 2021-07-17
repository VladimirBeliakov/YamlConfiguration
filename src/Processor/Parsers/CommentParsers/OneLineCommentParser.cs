using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class OneLineCommentParser : IOneLineCommentParser
	{
		private const int _commentCharLength = 1;

		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var chars = await charStream.Peek(_commentCharLength + Characters.CharGroupMaxLength).ConfigureAwait(false);

			var whiteCharsSkipped = 0;

			foreach (var @char in chars)
				if (@char == Characters.Tab || @char == Characters.Space)
				{
					whiteCharsSkipped++;
				}
				else
				{
					if (@char == Characters.Comment)
						break;

					return false;
				}

			if (whiteCharsSkipped > Characters.CharGroupMaxLength)
				throw new InvalidYamlException(
					$"Too many white space characters in the comment line. " +
					$"Allowed is {Characters.CharGroupMaxLength}."
				);

			if (whiteCharsSkipped == chars.Count)
				return false;

			var readLine = await charStream.ReadLine().ConfigureAwait(false);
			var commentLength = readLine.Length - whiteCharsSkipped;

			const int allowedCommentLength = _commentCharLength + Characters.CommentTextMaxLength;

			if (commentLength > allowedCommentLength)
				throw new InvalidYamlException($"Too long comment. Allowed length is {allowedCommentLength}.");

			return true;
		}
	}
}