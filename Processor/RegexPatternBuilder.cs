using System.Collections.Generic;

namespace Processor
{
	internal static class RegexPatternBuilder
	{
		public static string BuildWithExclusiveChars(string exclusiveChars, string inclusiveChars) =>
			$"(?:(?![{exclusiveChars}]){inclusiveChars})";
	}
}