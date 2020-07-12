namespace Parser
{
	public static class Directives
	{
		private static readonly string _reservedDirectiveName =
			$"(?:{Characters.NonSpaceChar}){{1,{Characters.CharGroupLength}}}";
		private static readonly string _parameter = $"(?:{Characters.NonSpaceChar}){{1,{Characters.CharGroupLength}}}";

		public static readonly string Reserved =
			$"^{Characters.Directive}{_reservedDirectiveName}" +
			$"{BasicStructures.SeparateInLine}{_parameter}" +
			$"{BasicStructures.Comment}";
	}
}