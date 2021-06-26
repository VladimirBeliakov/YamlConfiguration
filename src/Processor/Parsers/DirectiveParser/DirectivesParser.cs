using System.Collections.Generic;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DirectivesParser : IDirectivesParser
	{
		private readonly IReadOnlyCollection<IOneDirectiveParser> _directiveParsers;

		public DirectivesParser(IReadOnlyCollection<IOneDirectiveParser> directiveParsers)
		{
			_directiveParsers = directiveParsers;
		}

		public async ValueTask<DirectiveParseResult> Process(ICharacterStream charStream)
		{
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