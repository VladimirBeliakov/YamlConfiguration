using System;
using System.Text.RegularExpressions;

namespace YamlConfiguration.Processor.TypeDefinitions
{
	public class TypeDefiner
	{
		public Type Define(string value)
		{
			throw new NotImplementedException();
		}
		
		private static readonly Regex _yamlMappingRegex = new Regex(
			$"^([\\w]{{1,{Characters.CharGroupMaxLength}}}):{BasicStructures.Spaces}" +
			$"(.{{1,{Characters.CharGroupMaxLength}}}?)" +
			$"{BasicStructures.Comment}",
			RegexOptions.Compiled
		);
	}
}