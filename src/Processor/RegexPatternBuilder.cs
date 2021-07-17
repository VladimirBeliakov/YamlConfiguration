using System;
using System.Collections.Generic;

namespace YamlConfiguration.Processor
{
	internal static class RegexPatternBuilder
	{
		public static RegexPattern BuildExclusive(string exclusiveChars, string inclusiveChars) =>
			new($"(?![{exclusiveChars}]){inclusiveChars}");

		public static RegexPattern BuildCharSet(params string[] items) => new($"[{join(items)}]");

		public static RegexPattern BuildNegatedCharSet(params string[] items) => new($"[^{join(items)}]");

		public static RegexPattern BuildAlternation(params string[] items) => new($"(?:{join(items, separator: "|")})");

		public static string BuildLookAhead(string beforeChars, RegexPattern lookAheadExpression) =>
			$"{beforeChars}(?={lookAheadExpression.ToString()})";

		public static string BuildLookBehind(RegexPattern lookBehindExpression, string afterChars) =>
			$"(?<={lookBehindExpression.ToString()}){afterChars}";

		public static RegexPattern AsNonCapturingGroup(this RegexPattern pattern) =>
			new($"(?:{pattern.ToString()})");

		public static RegexPattern AsCapturingGroup(this RegexPattern pattern) =>
			new($"({pattern.ToString()})");

		public static RegexPattern AsOptional(this RegexPattern pattern, bool asNonCapturingGroup = true) =>
			new($"{(asNonCapturingGroup ? pattern.AsNonCapturingGroup().ToString() : pattern)}?");

		public static RegexPattern WithLimitingRepetition(
			this RegexPattern pattern,
			int min = 0,
			int max = Characters.CharGroupMaxLength,
			bool asNonCapturingGroup = true
		) => new(
				$"{(asNonCapturingGroup ? pattern.AsNonCapturingGroup().ToString() : pattern)}" +
				$"{{{min.ToString()},{max.ToString()}}}"
			 );

		public static RegexPattern WithAnchorAtBeginning(this RegexPattern pattern) =>
			new($"^{pattern.ToString()}");

		public static RegexPattern WithAnchorAtEnd(this RegexPattern pattern) =>
			new($"{pattern.ToString()}$");

		private static string join(IEnumerable<string> items, string? separator = null) =>
			String.Join(separator ?? String.Empty, items);
	}
}
