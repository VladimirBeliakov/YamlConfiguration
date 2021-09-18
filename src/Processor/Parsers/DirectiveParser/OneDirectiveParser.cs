using System;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal abstract class OneDirectiveParser : IOneDirectiveParser
	{
		private readonly IMultiLineCommentParser _multiLineCommentParser;

		protected OneDirectiveParser(IMultiLineCommentParser multiLineCommentParser)
		{
			_multiLineCommentParser = multiLineCommentParser;
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
			{
				var isComment = await _multiLineCommentParser.TryProcess(charStream).ConfigureAwait(false);

				if (!isComment)
					throw new InvalidYamlException("Only a comment may follow a directive.");
			}

			return directive;
		}

		protected abstract IDirective? Parse(string rawDirective);

		protected abstract string DirectiveName { get; }

		protected void LogParseFailure(string notParsedDirective) =>
			Console.WriteLine($"Failed to parse {DirectiveName.ToLower()} directive '{notParsedDirective}'.");

		private async ValueTask<bool> checkDirectiveName(ICharacterStream charStream)
		{
			const int directiveCharLength = 1;

			var directiveCharAndNameLength = directiveCharLength + DirectiveName.Length;

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