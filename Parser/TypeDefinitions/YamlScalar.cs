using System;
using System.Text.RegularExpressions;
using Parser.Exceptions;

namespace Parser.TypeDefinitions
{
	public class YamlScalar
	{
		public string Value { get; }
		
		public YamlScalar(string scalar, bool isCollectionItem = false)
		{
			var match = isCollectionItem ? _yamlCollectionScalarRegex.Match(scalar) : _yamlScalarRegex.Match(scalar);
			if (!match.Success)
				throw new InvalidYamlCollectionItemException(
					$"{nameof(scalar)} '{scalar}' has invalid format. Should be '(  )- ItemName( # comment)'");

			Value = match.Groups[1].Value;
		}

		private static readonly Regex _yamlScalarRegex =
			new Regex(
				$"^\\- ([\\w]{{1,{GlobalConstants.CharSequenceLength}}}){GlobalConstants.CommentRegex}",
				RegexOptions.Compiled
			);
		
		private static readonly Regex _yamlCollectionScalarRegex = 
			new Regex(
				$@"{_yamlScalarRegex.ToString().Replace(
					"^", $"^{GlobalConstants.SpacesRegex}", StringComparison.Ordinal)}"
			);
	}
}