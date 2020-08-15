namespace Processor
{
	public static class Directives
	{
		private const string _yamlDirectiveName = "YAML";
		private const string _tagDirectiveName = "TAG";

		private static readonly string _reservedDirectiveName =
			$"((?:{Characters.NonSpaceChar}){{1,{Characters.CharGroupLength}}})";

		private static readonly string _parameter =
			$"((?:{Characters.NonSpaceChar}){{1,{Characters.CharGroupLength}}})";

		private static readonly string _tagHandle =
			$"({Characters.Tag}{Characters.WordChar}{{0,{Characters.CharGroupLength}}}{Characters.Tag}?)";

		private static readonly string _localTagPrefix =
			$"{Characters.Tag}{Characters.UriChar}{{0,{Characters.CharGroupLength}}}";

		private static readonly string _globalTagPrefix =
			$"{Characters.TagChar}{Characters.UriChar}{{0,{Characters.CharGroupLength}}}";

		private static readonly string _tagPrefix = $"({_localTagPrefix}|{_globalTagPrefix})";

		public static readonly string Reserved =
			$"^{Characters.Directive + _reservedDirectiveName}" +
			$"{BasicStructures.SeparateInLine + _parameter}" +
			$"{BasicStructures.Comment}";

		public static readonly string Yaml =
			$"^{Characters.Directive + _yamlDirectiveName}" +
			$"{BasicStructures.SeparateInLine}" +
			$"([{Characters.DecimalDigits}]{{1,{Characters.CharGroupLength}}}" +
			$"{Characters.VersionSeparator}" +
			$"[{Characters.DecimalDigits}]{{1,{Characters.CharGroupLength}}})" +
			$"{BasicStructures.Comment}";

		public static readonly string Tag =
			$"^{Characters.Directive + _tagDirectiveName}" +
			$"{BasicStructures.SeparateInLine}" +
			$"{_tagHandle}" +
			$"{BasicStructures.SeparateInLine}" +
			$"{_tagPrefix}" +
			$"{BasicStructures.Comment}";
	}
}