using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class PlainInOneLineParser : IPlainInOneLineParser
	{
		private static readonly ushort _breakCharLength = 1;

		private static readonly Regex _blockKeyOneLineRegex =
			new(Plain.GetPatternFor(Context.BlockKey), RegexOptions.Compiled);

		private static readonly Regex _flowKeyOneLineRegex =
			new(Plain.GetPatternFor(Context.FlowKey), RegexOptions.Compiled);

		public async ValueTask<PlainLineNode?> TryProcess(
			ICharacterStream charStream,
			Context context,
			bool asOneLine = false
		)
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
				var valueLength = value.Length;

				if (asOneLine)
				{
					var valueWithBreakLength = value.Length + _breakCharLength;

					if (valueWithBreakLength != peekedLine.Length)
						return null;

					valueLength = valueWithBreakLength;
				}

				await charStream.AdvanceBy((uint) valueLength).ConfigureAwait(false);

				return new PlainLineNode(value);
			}

			return null;
		}
	}
}
