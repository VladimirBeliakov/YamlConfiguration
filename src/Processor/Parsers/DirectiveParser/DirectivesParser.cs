using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DirectivesParser : IDirectivesParser
	{
		private readonly IReadOnlyCollection<IOneDirectiveParser> _directiveParsers;
		private readonly IOneLineCommentParser _oneLineCommentParser;

		public DirectivesParser(
			IReadOnlyCollection<IOneDirectiveParser> directiveParsers,
			IOneLineCommentParser oneLineCommentParser
		)
		{
			_directiveParsers = directiveParsers;
			_oneLineCommentParser = oneLineCommentParser;
		}

		public async ValueTask<DirectiveParseResult> Process(ICharacterStream charStream)
		{
			var possibleDirectiveChar = await charStream.Peek().ConfigureAwait(false);

			if (possibleDirectiveChar != '%')
				return new DirectiveParseResult(Array.Empty<IDirective>());

			var directives = new List<IDirective>();

			bool anyNewDirectives;
			do
			{
				anyNewDirectives = false;

				foreach (var directiveParser in _directiveParsers)
				{
					var directive = await directiveParser.Process(charStream);

					if (directive is null)
						continue;

					anyNewDirectives = true;

					directives.Add(directive);

					while (await _oneLineCommentParser.TryProcess(charStream).ConfigureAwait(false)) {}
				}
			} while (anyNewDirectives);

			var isDirectiveEndPresent = await tryReadDirectiveEnd(charStream).ConfigureAwait(false);

			return new DirectiveParseResult(directives, isDirectiveEndPresent);
		}

		private static async ValueTask<bool> tryReadDirectiveEnd(ICharacterStream charStream)
		{
			const int directiveEndLength = 4;

			var possibleDirectiveEndChars = await charStream.Peek(directiveEndLength).ConfigureAwait(false);

			if (possibleDirectiveEndChars.Count < directiveEndLength)
				return false;

			var @break = BasicStructures.Break;
			const char dash = '-';

			if (
				possibleDirectiveEndChars[0] == dash &&
				possibleDirectiveEndChars[1] == dash &&
				possibleDirectiveEndChars[2] == dash &&
				possibleDirectiveEndChars[3] == @break
			)
			{
				for (var i = 0; i < directiveEndLength; i++)
					await charStream.Read().ConfigureAwait(false);

				return true;
			}

			return false;
		}
	}
}