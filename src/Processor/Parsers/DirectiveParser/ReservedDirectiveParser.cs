using System;
using System.Text.RegularExpressions;

namespace YamlConfiguration.Processor
{
	internal class ReservedDirectiveParser : OneDirectiveParser
	{
		private static readonly Regex _reservedDirectiveRegex = new(
			BasicStructures.Directives.Reserved,
			RegexOptions.Compiled
		);

		public ReservedDirectiveParser(ICommentParser commentParser) : base(commentParser) {}

		protected override string DirectiveName => String.Empty;

		protected override IDirective? Parse(string rawDirective)
		{
			var result = _reservedDirectiveRegex.Match(rawDirective);

			if (!result.Success)
			{
				LogParseFailure(rawDirective);
				return null;
			}

			var name = result.Groups[1].Captures[0].Value;
			var parameter = result.Groups[2].Captures[0].Value;

			return new ReservedDirective(name: name, parameter: parameter);
		}
	}
}