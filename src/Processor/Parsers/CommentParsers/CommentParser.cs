using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class CommentParser : ICommentParser
	{
		private const int _commentCharLength = 1;
		private const int _allowedCommentLength = _commentCharLength + Characters.CommentTextMaxLength;

		public async ValueTask<bool> TryProcess(ICharacterStream charStream)
		{
			var possibleCommentChar = await charStream.Peek().ConfigureAwait(false);

			if (possibleCommentChar != Characters.Comment)
				return false;

			var readLine = await charStream.ReadLine().ConfigureAwait(false);
			var commentLength = readLine.Length;

			if (commentLength > _allowedCommentLength)
				throw new InvalidYamlException($"Too long comment. Allowed {_allowedCommentLength} chars.");

			return true;
		}
	}
}