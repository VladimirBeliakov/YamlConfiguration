using System;
using System.Text.RegularExpressions;

namespace YamlConfiguration.Processor
{
	internal class TagDirectiveParser : OneDirectiveParser
	{
		private static readonly Regex _tagDirectiveRegex = new(
			BasicStructures.Directives.Tag,
			RegexOptions.Compiled
		);

		public TagDirectiveParser(ICommentParser commentParser) : base(commentParser)
		{
		}

		protected override string DirectiveName => BasicStructures.Directives.TagDirectiveName;

		protected override IDirective? Parse(string rawDirective)
		{
			var result = _tagDirectiveRegex.Match(rawDirective);

			if (!result.Success)
			{
				LogParseFailure(rawDirective);
				return null;
			}

			var handle = result.Groups[1].Captures[0].Value;
			var prefix = result.Groups[2].Captures[0].Value;

			return new TagDirective(handle: handle, prefix: prefix);
		}
	}
}