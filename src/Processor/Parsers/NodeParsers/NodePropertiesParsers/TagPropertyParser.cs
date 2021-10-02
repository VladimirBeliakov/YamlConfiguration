using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class TagPropertyParser : ITagPropertyParser
	{
		private static readonly Regex _verbatimTagRegex =
			new(BasicStructures.NodeTags.VerbatimTag, RegexOptions.Compiled);

		private static readonly Regex _shorthandTagRegex =
			new(BasicStructures.NodeTags.ShorthandTag, RegexOptions.Compiled);

		private static readonly Regex _nonSpecificTagRegex =
			new(BasicStructures.NodeTags.NonSpecificTag, RegexOptions.Compiled);

		public async ValueTask<TagProperty?> Process(ICharacterStream charStream)
		{
			var possibleTagIndicator = await charStream.Peek().ConfigureAwait(false);

			if (possibleTagIndicator != Characters.Tag)
				return null;

			var peekedLine = await charStream.PeekLine().ConfigureAwait(false);

			var match = _verbatimTagRegex.Match(peekedLine);

			if (match.Success)
				return await createTagProperty(TagType.Verbatim).ConfigureAwait(false);

			match = _shorthandTagRegex.Match(peekedLine);

			if (match.Success)
				return await createTagProperty(TagType.Shorthand).ConfigureAwait(false);

			match = _nonSpecificTagRegex.Match(peekedLine);

			if (match.Success)
				return await createTagProperty(TagType.NonSpecific).ConfigureAwait(false);

			return null;

			async ValueTask<TagProperty> createTagProperty(TagType type)
			{
				var tag = match.Groups[1].Captures[0].Value;

				await charStream.AdvanceBy((uint) tag.Length).ConfigureAwait(false);

				return new TagProperty { Type = type, Value = tag };
			}
		}
	}
}