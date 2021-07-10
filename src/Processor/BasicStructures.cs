using System;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	public static class BasicStructures
	{
		// TODO: Define the break code by the file parsed.
		internal static readonly RegexPattern Break = (RegexPattern) '\n';

		internal static readonly RegexPattern Spaces = Characters.Space.WithLimitingRepetition(min: 1);

		private static readonly RegexPattern _indent = Characters.Space.WithLimitingRepetition();

		private static readonly RegexPattern _anchoredIndent = _indent.WithAnchorAtBeginning();

		internal static readonly RegexPattern SeparateInLine =
			RegexPatternBuilder.BuildAlternation(
				RegexPattern.Empty.WithAnchorAtBeginning(),
				RegexPatternBuilder.BuildCharSet(Characters.Space, Characters.Tab)
					.WithLimitingRepetition(min: 1, asNonCapturingGroup: false)
			);

		private static readonly RegexPattern _tagHandle =
			Characters.Tag + Characters.WordChar.WithLimitingRepetition() + Characters.Tag.AsOptional();

		internal static RegexPattern LinePrefix(BlockFlow c, bool useAnchoredIndent = true)
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

		internal static RegexPattern EmptyLine(BlockFlow c, bool useAnchoredIndent = true)
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

		internal static readonly RegexPattern Comment =
			(
				SeparateInLine +
				(
					Characters.Comment +
					RegexPatternBuilder.BuildNegatedCharSet(Break)
						.WithLimitingRepetition(max: Characters.CharGroupLength * Characters.CharGroupLength)
				).AsOptional()
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

		internal static class Directives
		{
			public static RegexPattern YamlDirectiveName = (RegexPattern) "YAML";
			public static RegexPattern TagDirectiveName = (RegexPattern) "TAG";

			private static readonly RegexPattern _reservedDirectiveName =
				Characters.NsChar.AsNonCapturingGroup().WithLimitingRepetition(min: 1).AsCapturingGroup();

			private static readonly RegexPattern _parameter =
				Characters.NsChar.AsNonCapturingGroup().WithLimitingRepetition(min: 1).AsCapturingGroup();

			private static readonly RegexPattern _localTagPrefix =
				Characters.Tag + Characters.UriChar.WithLimitingRepetition();

			private static readonly string _globalTagPrefix =
				Characters.TagChar + Characters.UriChar.WithLimitingRepetition();

			private static readonly RegexPattern _tagPrefix = RegexPatternBuilder
				.BuildAlternation(_localTagPrefix, _globalTagPrefix).AsCapturingGroup();

			public static readonly RegexPattern Reserved =
				(
					Characters.Directive +
					_reservedDirectiveName +
					SeparateInLine +
					_parameter +
					Comment
				).WithAnchorAtBeginning();

			public static readonly RegexPattern Yaml =
				Characters.Directive.WithAnchorAtBeginning() +
				YamlDirectiveName +
				SeparateInLine +
				(
					RegexPatternBuilder.BuildCharSet(Characters.DecimalDigits).WithLimitingRepetition(min: 1) +
					Characters.Escape + Characters.VersionSeparator +
					RegexPatternBuilder.BuildCharSet(Characters.DecimalDigits).WithLimitingRepetition(min: 1)
				).AsCapturingGroup() +
				Comment;

			public static readonly string Tag =
				(
					Characters.Directive +
					 TagDirectiveName +
					 SeparateInLine +
					 _tagHandle.AsCapturingGroup() +
					 SeparateInLine +
					 _tagPrefix +
					 Comment
				).WithAnchorAtBeginning();
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
