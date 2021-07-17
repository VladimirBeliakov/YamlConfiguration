using System.Text.RegularExpressions;
using YamlConfiguration.Processor.Exceptions;

namespace YamlConfiguration.Processor.TypeDefinitions
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
				$"^\\- ([\\w]{{1,{Characters.CharGroupMaxLength}}})(?:{BasicStructures.Comment})?$",
				RegexOptions.Compiled
			);
		
		private static readonly Regex _yamlCollectionScalarRegex = 
			new Regex(
				$"^{BasicStructures.Spaces}\\- ([\\w]{{1,{Characters.CharGroupMaxLength}}})" +
				$"(?:{BasicStructures.Comment})?$",
				RegexOptions.Compiled
			);
	}
}