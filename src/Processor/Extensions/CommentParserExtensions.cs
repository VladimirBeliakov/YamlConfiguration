using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal static class CommentParserExtensions
	{
		public static async ValueTask ProcessLineComments(
			this ICommentParser commentParser,
			ICharacterStream charStream
		)
		{
			while (await commentParser.TryProcess(charStream, isLineComment: true).ConfigureAwait(false)) {}
		}
	}
}