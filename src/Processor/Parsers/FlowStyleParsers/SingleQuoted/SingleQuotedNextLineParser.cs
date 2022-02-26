using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class SingleQuotedNextLineParser
	{
		private const uint _singleQuoteLength = 1;

		private static readonly Regex _flowInNextLineRegex =
			new(SingleQuotedStyle.MultiLine.GetNextLinePatternFor(Context.FlowIn));

		private static readonly Regex _flowOutNextLineRegex =
			new(SingleQuotedStyle.MultiLine.GetNextLinePatternFor(Context.FlowOut));

		public async Task<SingleQuotedMultilineNode?> Process(ICharacterStream charStream, Context context)
		{
			var regex = context switch
			{
				Context.FlowIn => _flowInNextLineRegex,
				Context.FlowOut => _flowOutNextLineRegex,
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

			var isNextLineClosed = closingWhites is not null;

			if (isNextLineClosed)
			{
				var nextLineLength = content.Length + closingWhites!.Length + _singleQuoteLength;

				await charStream.AdvanceBy((uint) nextLineLength!).ConfigureAwait(false);
			}
			else
			{
				await charStream.AdvanceBy((uint) content.Length).ConfigureAwait(false);
			}

			return new SingleQuotedMultilineNode($"{content}{closingWhites}", isNextLineClosed);
		}
	}
}
