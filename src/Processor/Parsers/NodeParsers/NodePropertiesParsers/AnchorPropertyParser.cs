using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class AnchorPropertyParser
	{
		private static readonly Regex _anchorPropertyRegex =
			new(BasicStructures.NodeTags.AnchorProperty, RegexOptions.Compiled);

		public async ValueTask<AnchorProperty?> Process(ICharacterStream charStream)
		{
			var possibleAnchorChar = await charStream.Peek().ConfigureAwait(false);

			if (possibleAnchorChar != Characters.Anchor)
				return null;

			var peekedLine = await charStream.PeekLine().ConfigureAwait(false);

			var match = _anchorPropertyRegex.Match(peekedLine);

			if (match.Success)
			{
				var anchorName = match.Groups[1].Captures[0].Value;

				const int anchorCharLength = 1;

				await charStream.AdvanceBy(anchorCharLength + (uint) anchorName.Length).ConfigureAwait(false);

				return new AnchorProperty { AnchorName = anchorName };
			}

			return null;
		}
	}
}