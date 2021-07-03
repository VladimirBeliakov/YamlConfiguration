using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal abstract class OneDirectiveParser : IOneDirectiveParser
	{
		private readonly IOneLineCommentParser _oneLineCommentParser;

		protected OneDirectiveParser(IOneLineCommentParser oneLineCommentParser)
		{
			_oneLineCommentParser = oneLineCommentParser;
		}

		public async ValueTask<IDirective?> Process(ICharacterStream charStream)
		{
			var possibleDirectiveChar = await charStream.Peek().ConfigureAwait(false);

			if (possibleDirectiveChar != Characters.Directive)
				return null;

			if (!await checkDirective(charStream).ConfigureAwait(false))
				return null;

			var rawDirective = await charStream.ReadLine().ConfigureAwait(false);

			var directive = Parse(rawDirective);

			if (directive is not null)
				while (await _oneLineCommentParser.TryProcess(charStream).ConfigureAwait(false)) {}

			return directive;
		}

		protected abstract IDirective? Parse(string rawDirective);

		protected abstract string DirectiveName { get; }

		private async ValueTask<bool> checkDirective(ICharacterStream charStream)
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