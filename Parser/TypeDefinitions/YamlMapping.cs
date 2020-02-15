using System.Collections.Generic;
using System.Text.RegularExpressions;
using Parser.Exceptions;

namespace Parser.TypeDefinitions
{
	public class YamlMapping
	{
		public KeyValuePair<string, string> Pair { get; }

		public YamlMapping(string keyValuePair)
		{
			var match = _yamlMappingRegex.Match(keyValuePair);

			if (!match.Success)
				throw new InvalidYamlMappingException(
					$"{nameof(keyValuePair)} '{keyValuePair}' has invalid format. Should be 'key: value( # comment)'");
			
			Pair = new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value);
		}

		private static readonly Regex _yamlMappingRegex = new Regex(
			$"^([\\w]{{1,{GlobalConstants.CharSequenceLength}}}):{GlobalConstants.SpacesRegex}" +
			$"(.{{1,{GlobalConstants.CharSequenceLength}}}?)" +
			$"{GlobalConstants.CommentRegex}",
			RegexOptions.Compiled
		);
	}
}