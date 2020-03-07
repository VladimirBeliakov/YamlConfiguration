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
			$"^([\\w]{{1,{GlobalConstants.CharSequenceLength}}}):{GlobalConstants.SpacesRegex}" +
			$"(.{{1,{GlobalConstants.CharSequenceLength}}}?)" +
			$"{GlobalConstants.CommentRegex}",
			RegexOptions.Compiled
		);
	}
}