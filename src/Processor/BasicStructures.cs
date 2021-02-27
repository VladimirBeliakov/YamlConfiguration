using System;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	public static class BasicStructures
	{
		// TODO: Define the break code by the file parsed.
		public static readonly RegexPattern Break = (RegexPattern) Environment.NewLine;

		public static readonly RegexPattern Spaces = Characters.Space.WithLimitingRepetition(min: 1);

		private static readonly RegexPattern _indent = Characters.Space.WithLimitingRepetition();

		private static readonly RegexPattern _anchoredIndent = _indent.WithAnchorAtBeginning();

		internal static readonly RegexPattern SeparateInLine =
			(RegexPattern) $"(?:^|[{Characters.Space}{Characters.Tab}]{{1,{Characters.CharGroupLength}}})";

		private static readonly string _tagHandle =
			$"{Characters.Tag}{Characters.WordChar}{{0,{Characters.CharGroupLength}}}{Characters.Tag}?";

		public static RegexPattern LinePrefix(BlockFlow c, bool useAnchoredIndent = true)
		{
			var indent = useAnchoredIndent ? _anchoredIndent : _indent;

			switch (c)
			{
				case BlockFlow.BlockIn:
				case BlockFlow.BlockOut:
					return indent;
				case BlockFlow.FlowIn:
				case BlockFlow.FlowOut:
					return indent + SeparateInLine.AsOptional();
				default:
					throw new ArgumentOutOfRangeException(nameof(c), c, $"Unknown {nameof(BlockFlow)} item {c}.");
			}
		}

		public static RegexPattern EmptyLine(BlockFlow c, bool useAnchoredIndent = true)
		{
			return LinePrefix(c, useAnchoredIndent) + Break;
		}

		#region Folded Line Regexes

		public static string TrimmedLine(BlockFlow c)
		{
			// TODO: Since I'm going to process streams "line-by-line", multiline regex will be deleted and
			// their logic will be moved to upper levels.
			throw new NotSupportedException();
			return $"{Break}" +
				   $"(?:{EmptyLine(c, useAnchoredIndent: false)})+";
		}

		// TODO: When writing the processor, one matter should be observed:
		// if line breaks within a block surround a more intended line, then folding doesn't apply to such breaks.
		public static string BreakAsSpace(string linePrefix = "")
		{
			// TODO: Since I'm going to process streams "line-by-line", multiline regex will be deleted and
			// their logic will be moved to upper levels.
			throw new NotSupportedException();
			return Break +
				linePrefix +
				$"(?=.{{0,{Characters.CharGroupLength}}}[^ \t{Break}]{{1,{Characters.CharGroupLength}}}.{{0,{Characters.CharGroupLength}}})";
		}

		public static string FlowFoldedTrimmedLine()
		{
			// TODO: Since I'm going to process streams "line-by-line", multiline regex will be deleted and
			// their logic will be moved to upper levels.
			throw new NotSupportedException();
			return $"{SeparateInLine}?" +
			TrimmedLine(BlockFlow.FlowIn) +
			LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false);
		}

		public static string FlowFoldedLineWithBreakAsSpace()
		{
			// TODO: Since I'm going to process streams "line-by-line", multiline regex will be deleted and
			// their logic will be moved to upper levels.
			throw new NotSupportedException();
			return $"{SeparateInLine}?" +
			BreakAsSpace(linePrefix: LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false));
		}

		#endregion

		public static readonly string Comment =
			(SeparateInLine +
			 (Characters.Comment + RegexPatternBuilder.BuildNegatedCharSet(Break).WithLimitingRepetition()).AsOptional()
			).AsOptional() + Break;

		// TODO: Move the logic to a higher level.
		public static string SeparateLines(BlockFlow c)
		{
			// TODO: Since I'm going to process streams "line-by-line", multiline regex will be deleted and
			// their logic will be moved to upper levels.
			throw new NotSupportedException();
			switch (c)
			{
				case BlockFlow.BlockIn:
				case BlockFlow.BlockOut:
				case BlockFlow.FlowIn:
				case BlockFlow.FlowOut:
					return $"(?:{Comment}" +
						   $"(?:{Comment})*" +
						   $"{LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false)}" +
						   "|" +
						   $"{SeparateInLine})";
//				case BlockFlow.BlockKey:
//				case BlockFlow.FlowKey:
//					return _separateInLine;
				default:
					throw new ArgumentOutOfRangeException(nameof(c), c, null);
			}
		}

		public static class Directives
		{
			private const string _yamlDirectiveName = "YAML";
			private const string _tagDirectiveName = "TAG";

			private static readonly string _reservedDirectiveName =
				$"({Characters.NsChar.AsNonCapturingGroup()}{{1,{Characters.CharGroupLength}}})";

			private static readonly string _parameter =
				$"({Characters.NsChar.AsNonCapturingGroup()}{{1,{Characters.CharGroupLength}}})";

			private static readonly string _localTagPrefix =
				$"{Characters.Tag}{Characters.UriChar}{{0,{Characters.CharGroupLength}}}";

			private static readonly string _globalTagPrefix =
				$"{Characters.TagChar}{Characters.UriChar}{{0,{Characters.CharGroupLength}}}";

			private static readonly string _tagPrefix = $"({_localTagPrefix}|{_globalTagPrefix})";

			public static readonly string Reserved =
				$"^{Characters.Directive + _reservedDirectiveName}" +
				$"{SeparateInLine + _parameter}" +
				$"{Comment}";

			public static readonly string Yaml =
				$"^{Characters.Directive + _yamlDirectiveName}" +
				$"{SeparateInLine}" +
				$"([{Characters.DecimalDigits}]{{1,{Characters.CharGroupLength}}}" +
				$"{Characters.VersionSeparator}" +
				$"[{Characters.DecimalDigits}]{{1,{Characters.CharGroupLength}}})" +
				$"{Comment}";

			public static readonly string Tag =
				$"^{Characters.Directive + _tagDirectiveName}" +
				$"{SeparateInLine}" +
				$"({_tagHandle})" +
				$"{SeparateInLine}" +
				$"{_tagPrefix}" +
				$"{Comment}";
		}

		public static class NodeTags
		{
			// Even though '!<!>' satisfies the regex, it's not a valid verbatim tag since verbatim tags are not
			// subject to tag resolution.
			private static readonly string _verbatimTag = $"!<{Characters.UriChar}{{1,{Characters.CharGroupLength}}}>";

			private static readonly string _shorthandTag =
				$"{_tagHandle}{Characters.TagChar.AsNonCapturingGroup()}{{1,{Characters.CharGroupLength}}}";

			private static readonly string _nonSpecificTag = $"{Characters.Tag}";

			private static readonly string _tagProperty = $"({_verbatimTag}|{_shorthandTag}|{_nonSpecificTag})";

			internal static readonly string AnchorName =
				$"{Characters.AnchorChar.AsNonCapturingGroup()}{{1,{Characters.CharGroupLength}}}";

			private static readonly string _anchorProperty = $"{Characters.Anchor}({AnchorName})";

			public static readonly string TagAnchorProperties =
				$"^{SeparateInLine}{_tagProperty}(?:{SeparateInLine}{_anchorProperty})?{SeparateInLine}";

			public static readonly string AnchorTagProperties =
				$"^{SeparateInLine}{_anchorProperty}(?:{SeparateInLine}{_tagProperty})?{SeparateInLine}";
		}
	}
}
