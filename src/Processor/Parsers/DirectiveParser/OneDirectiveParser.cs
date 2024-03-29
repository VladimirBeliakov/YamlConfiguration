using System;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal abstract class OneDirectiveParser : IOneDirectiveParser
	{
		private readonly ICommentParser _commentParser;

		protected OneDirectiveParser(ICommentParser commentParser)
		{
			_commentParser = commentParser;
		}

		public async ValueTask<IDirective?> Process(ICharacterStream charStream)
		{
			var possibleDirectiveChar = await charStream.Peek().ConfigureAwait(false);

			if (possibleDirectiveChar != Characters.Directive)
				return null;

			if (DirectiveName.Length > 0 && !await checkDirectiveName(charStream).ConfigureAwait(false))
				return null;

			var rawDirective = await charStream.ReadLine().ConfigureAwait(false);

			var directive = Parse(rawDirective);

			if (directive is not null)
				await _commentParser.ProcessLineComments(charStream).ConfigureAwait(false);

			return directive;
		}

		protected abstract IDirective? Parse(string rawDirective);

		protected abstract string DirectiveName { get; }

		protected void LogParseFailure(string notParsedDirective) =>
			Console.WriteLine($"Failed to parse {DirectiveName.ToLower()} directive '{notParsedDirective}'.");

		private async ValueTask<bool> checkDirectiveName(ICharacterStream charStream)
		{
			const int directiveCharLength = 1;

			var directiveCharAndNameLength = directiveCharLength + (uint) DirectiveName.Length;

			var possibleDirectiveChars = await charStream.Peek(directiveCharAndNameLength).ConfigureAwait(false);

			if (possibleDirectiveChars.Count != directiveCharAndNameLength)
				return false;

			for (var i = directiveCharLength; i < possibleDirectiveChars.Count; i++)
				if (possibleDirectiveChars[i] != DirectiveName[i - directiveCharLength])
					return false;

			return true;
		}
	}
}