using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class SingleQuotedInOneLineParser
	{
		private static readonly Regex _blockKeyOneLineRegex =
			new(SingleQuotedStyle.GetPatternFor(Context.BlockKey), RegexOptions.Compiled);

		private static readonly Regex _flowKeyOneLineRegex =
			new(SingleQuotedStyle.GetPatternFor(Context.FlowKey), RegexOptions.Compiled);

		private static readonly Regex _flowInOneLineRegex =
			new(SingleQuotedStyle.GetPatternFor(Context.FlowIn), RegexOptions.Compiled);

		private static readonly Regex _flowOutOneLineRegex =
			new(SingleQuotedStyle.GetPatternFor(Context.FlowOut), RegexOptions.Compiled);

		public async ValueTask<SingleQuotedLineNode?> Process(ICharacterStream charStream, Context context)
		{
			var regex = context switch
			{
				Context.BlockKey => _blockKeyOneLineRegex,
				Context.FlowKey => _flowKeyOneLineRegex,
				Context.FlowIn => _flowInOneLineRegex,
				Context.FlowOut => _flowOutOneLineRegex,
				_ => throw new ArgumentOutOfRangeException(
						nameof(context),
						context,
						$"Only {Context.BlockKey}, {Context.FlowKey}, " +
						$"{Context.FlowIn}, and {Context.FlowOut} are supported."
					),
			};

			var peekedLine = await charStream.PeekLine().ConfigureAwait(false);

			var match = regex.Match(peekedLine);

			if (!match.Success)
				return null;

			await charStream.AdvanceBy((uint) match.Value.Length).ConfigureAwait(false);

			return new SingleQuotedLineNode(match.Groups[1].Captures[0].Value);
		}
	}
}
