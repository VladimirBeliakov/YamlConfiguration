using System;
using System.Text.RegularExpressions;

namespace YamlConfiguration.Processor
{
	internal class YamlDirectiveParser : OneDirectiveParser
	{
		private static readonly Regex _yamlDirectiveRegex = new(
			BasicStructures.Directives.Yaml,
			RegexOptions.Compiled
		);

		public YamlDirectiveParser(IOneLineCommentParser oneLineCommentParser) : base(oneLineCommentParser) {}

		protected override string DirectiveName => BasicStructures.Directives.YamlDirectiveName;

		protected override IDirective? Parse(string rawDirective)
		{
			var result = _yamlDirectiveRegex.Match(rawDirective);

			if (!result.Success)
			{
				LogParseFailure(rawDirective);
				return null;
			}

			var yamlVersion = Version.Parse(result.Groups[1].Value);

			if (yamlVersion.Major is 0 or > 1)
			{
				Console.WriteLine($"Yaml directive major version can be 1 only.");
				return null;
			}

			if (yamlVersion.Minor > 2)
			{
				Console.WriteLine(
					$"Yaml directive minor version cannot be higher than 2. " +
					$"Current minor version {yamlVersion.Minor} will be ignored."
				);

				yamlVersion = new Version(1, 2);
			}

			return new YamlDirective(yamlVersion);
		}
	}
}