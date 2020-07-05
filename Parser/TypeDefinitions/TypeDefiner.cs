using System;
using System.Text.RegularExpressions;

namespace Parser.TypeDefinitions
{
	public class TypeDefiner
	{
		public Type Define(string value)
		{
			throw new NotImplementedException();
		}
		
		private static readonly Regex _yamlMappingRegex = new Regex(
			$"^([\\w]{{1,{BasicStructures.CharGroupLength}}}):{BasicStructures.Spaces}" +
			$"(.{{1,{BasicStructures.CharGroupLength}}}?)" +
			$"{BasicStructures.CommentRegex}",
			RegexOptions.Compiled
		);
	}
}