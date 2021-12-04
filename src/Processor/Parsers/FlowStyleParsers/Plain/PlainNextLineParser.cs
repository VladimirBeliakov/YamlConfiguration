using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class PlainNextLineParser : IPlainNextLineParser
	{
		private const uint _breakCharLength = 1;

		private readonly Regex _flowInNextLineRegex =
			new(Plain.NextLine.GetPatternFor(Context.FlowIn), RegexOptions.Compiled);
		private readonly Regex _flowOutNextLineRegex =
			new(Plain.NextLine.GetPatternFor(Context.FlowOut), RegexOptions.Compiled);

		public async ValueTask<string?> TryProcess(ICharacterStream charStream, Context context)
		{
			var regex = context switch
			{
				Context.FlowIn => _flowInNextLineRegex,
				Context.FlowOut => _flowOutNextLineRegex,
				_ => throw new ArgumentOutOfRangeException(
						nameof(context),
						context,
						$"Only {Context.FlowIn} and {Context.FlowOut} are supported."
					)
			};

			var peekedLine = await charStream.PeekLine().ConfigureAwait(false);

			var match = regex.Match(peekedLine);

			if (match.Success)
			{
				var value = match.Groups[1].Captures[0].Value;

				await charStream.AdvanceBy((uint) value.Length + _breakCharLength).ConfigureAwait(false);

				return value;
			}

			return null;
		}
	}
}
