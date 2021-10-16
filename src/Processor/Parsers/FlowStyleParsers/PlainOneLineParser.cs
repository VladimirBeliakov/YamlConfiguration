using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class PlainOneLineParser : IPlainOneLineParser
	{
		private static readonly Regex _blockKeyOneLineRegex =
			new(Plain.GetPatternFor(Context.BlockKey), RegexOptions.Compiled);

		private static readonly Regex _flowKeyOneLineRegex =
			new(Plain.GetPatternFor(Context.FlowKey), RegexOptions.Compiled);

		public async ValueTask<PlainOneLineNode?> Process(ICharacterStream charStream, Context context)
		{
			var regex = context switch
			{
				Context.BlockKey => _blockKeyOneLineRegex,
				Context.FlowKey => _flowKeyOneLineRegex,
				_ => throw new ArgumentOutOfRangeException(
						nameof(context),
						context,
						$"Only {Context.BlockKey} and {Context.FlowKey} are supported."
					)
			};

			var peekedLine = await charStream.PeekLine().ConfigureAwait(false);

			var match = regex.Match(peekedLine);

			if (match.Success)
			{
				var value = match.Value;

				await charStream.AdvanceBy((uint) value.Length).ConfigureAwait(false);

				return new PlainOneLineNode(value);
			}

			return null;
		}
	}
}