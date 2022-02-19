using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class SingleQuotedFirstLineParser
	{
		private const uint _singleQuoteLength = 1;

		private static readonly Regex _flowInFirstLineRegex =
			new(SingleQuotedStyle.MultiLine.GetFirstLinePatternFor(Context.FlowIn), RegexOptions.Compiled);

		private static readonly Regex _flowOutFirstLineRegex =
			new(SingleQuotedStyle.MultiLine.GetFirstLinePatternFor(Context.FlowOut), RegexOptions.Compiled);

		public async Task<SingleQuotedFirstLineNode?> Process(ICharacterStream charStream, Context context)
		{
			var regex = context switch
			{
				Context.FlowIn => _flowInFirstLineRegex,
				Context.FlowOut => _flowOutFirstLineRegex,
				_ => throw new ArgumentOutOfRangeException(
						nameof(context),
						context,
						$"Only {Context.FlowIn} and {Context.FlowOut} are allowed."
					),
			};

			var peekedLine = await charStream.PeekLine().ConfigureAwait(false);

			var match = regex.Match(peekedLine);

			if (!match.Success)
				return null;

			var content = match.Groups[1].Captures.First().Value;

			var closingWhites = match.Groups[2].Captures.FirstOrDefault()?.Value;

			var isFirstLineClosed = closingWhites is not null;

			if (isFirstLineClosed)
			{
				var firstLineLength =
					_singleQuoteLength + content.Length + closingWhites!.Length + _singleQuoteLength;

				await charStream.AdvanceBy((uint) firstLineLength!).ConfigureAwait(false);
			}
			else
			{
				await charStream.AdvanceBy(_singleQuoteLength + (uint) content.Length).ConfigureAwait(false);
			}

			return new SingleQuotedFirstLineNode($"{content}{closingWhites}", isFirstLineClosed);
		}
	}
}
