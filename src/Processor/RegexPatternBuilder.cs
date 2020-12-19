using System;
using System.Collections.Generic;

namespace Processor
{
	internal static class RegexPatternBuilder
	{
		public static RegexPattern BuildExclusive(string exclusiveChars, string inclusiveChars) =>
			new RegexPattern($"(?![{exclusiveChars}]){inclusiveChars}");

		public static RegexPattern BuildCharSet(params string[] items) => new RegexPattern($"[{join(items)}]");

		public static RegexPattern BuildNegatedCharSet(params string[] items) => new RegexPattern($"[^{join(items)}]");

		public static RegexPattern BuildAlternation(params string[] items) =>
			new RegexPattern($"(?:{join(items, separator: "|")})");

		public static string BuildLookAhead(string beforeChars, RegexPattern lookAheadExpression) =>
			$"{beforeChars}(?={lookAheadExpression})";

		public static string BuildLookBehind(RegexPattern lookBehindExpression, string afterChars) =>
			$"(?<={lookBehindExpression}){afterChars}";

		public static RegexPattern AsNonCapturingGroup(this RegexPattern pattern) => new RegexPattern($"(?:{pattern})");

		public static RegexPattern AsCapturingGroup(this RegexPattern pattern) => new RegexPattern($"({pattern})");

		public static RegexPattern AsOptional(this RegexPattern pattern) => new RegexPattern($"(?:{pattern})?");

		public static RegexPattern WithLimitingRepetition(
			this RegexPattern pattern,
			int min = 0,
			int max = Characters.CharGroupLength
		) => new RegexPattern($"(?:{pattern}){{{min},{max}}}");

		public static RegexPattern WithAnchorAtBeginning(this RegexPattern pattern) => new RegexPattern($"^{pattern}");

		public static RegexPattern WithAnchorAtEnd(this RegexPattern pattern) => new RegexPattern($"{pattern}$");

		private static string join(IEnumerable<string> items, string? separator = null) =>
			String.Join(separator ?? String.Empty, items);
	}
}
