namespace Parser
{
	public static class Directives
	{
		private static readonly string _reservedDirectiveName =
			$"((?:{Characters.NonSpaceChar}){{1,{Characters.CharGroupLength}}})";
		private static readonly string _parameter = $"((?:{Characters.NonSpaceChar}){{1,{Characters.CharGroupLength}}})";

		private const string _yamlDirectiveName = "YAML";

		public static readonly string Reserved =
			$"^{Characters.Directive + _reservedDirectiveName}" +
			$"{BasicStructures.SeparateInLine + _parameter}" +
			$"{BasicStructures.Comment}";

		public static readonly string Yaml =
			$"^{Characters.Directive + _yamlDirectiveName}" +
			$"{BasicStructures.SeparateInLine}" +
			$"[{Characters.DecimalDigits}]{{1,{Characters.CharGroupLength}}}" +
			$"{Characters.VersionSeparator}" +
			$"[{Characters.DecimalDigits}]{{1,{Characters.CharGroupLength}}}" +
			$"{BasicStructures.Comment}";
	}
}