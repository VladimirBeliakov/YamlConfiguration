using System;
using Processor.TypeDefinitions;
using static Processor.BasicStructures;

namespace Processor
{
	public static class BasicStructures
	{
		// TODO: Define the break code by the file parsed.
		public static readonly string Break = Environment.NewLine;

		public static readonly string Spaces = $"{Characters.SPACE}{{1,{Characters.CharGroupLength}}}";

		private static readonly string _indent = $"{Characters.SPACE}{{0,{Characters.CharGroupLength}}}";

		private static readonly string _anchoredIndent = $"^{_indent}";

		private static readonly string _separateInLine =
			$"(?:^|[{Characters.SPACE}{Characters.TAB}]{{1,{Characters.CharGroupLength}}})";

		private static readonly string _tagHandle =
			$"{Characters.Tag}{Characters.WordChar}{{0,{Characters.CharGroupLength}}}{Characters.Tag}?";

		public static string LinePrefix(BlockFlow c, bool useAnchoredIndent = true)
		{
			var indent = useAnchoredIndent ? _anchoredIndent : _indent;

			switch (c)
			{
				case BlockFlow.BlockIn:
				case BlockFlow.BlockOut:
					return indent;
				case BlockFlow.FlowIn:
				case BlockFlow.FlowOut:
					return $"{indent}{_separateInLine}?";
				default:
					throw new ArgumentOutOfRangeException(nameof(c), c, $"Unknown {nameof(BlockFlow)} item {c}.");
			}
		}

		public static string EmptyLine(BlockFlow c, bool useAnchoredIndent = true)
		{
			return $"{LinePrefix(c, useAnchoredIndent)}{Break}";
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
			return $"{_separateInLine}?" +
			TrimmedLine(BlockFlow.FlowIn) +
			LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false);
		}

		public static string FlowFoldedLineWithBreakAsSpace()
		{
			// TODO: Since I'm going to process streams "line-by-line", multiline regex will be deleted and
			// their logic will be moved to upper levels.
			throw new NotSupportedException();
			return $"{_separateInLine}?" +
			BreakAsSpace(linePrefix: LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false));
		}

		#endregion

		public static readonly string Comment =
			$"(?:{_separateInLine}(?:#[^{Break}]{{0,{Characters.CharGroupLength * Characters.CharGroupLength}}})?)?" +
			$"{Break}";

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
						   $"{_separateInLine})";
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
				$"({Characters.NonSpaceChar}{{1,{Characters.CharGroupLength}}})";

			private static readonly string _parameter =
				$"({Characters.NonSpaceChar}{{1,{Characters.CharGroupLength}}})";

			private static readonly string _localTagPrefix =
				$"{Characters.Tag}{Characters.UriChar}{{0,{Characters.CharGroupLength}}}";

			private static readonly string _globalTagPrefix =
				$"{Characters.TagChar}{Characters.UriChar}{{0,{Characters.CharGroupLength}}}";

			private static readonly string _tagPrefix = $"({_localTagPrefix}|{_globalTagPrefix})";

			public static readonly string Reserved =
				$"^{Characters.Directive + _reservedDirectiveName}" +
				$"{_separateInLine + _parameter}" +
				$"{Comment}";

			public static readonly string Yaml =
				$"^{Characters.Directive + _yamlDirectiveName}" +
				$"{_separateInLine}" +
				$"([{Characters.DecimalDigits}]{{1,{Characters.CharGroupLength}}}" +
				$"{Characters.VersionSeparator}" +
				$"[{Characters.DecimalDigits}]{{1,{Characters.CharGroupLength}}})" +
				$"{Comment}";

			public static readonly string Tag =
				$"^{Characters.Directive + _tagDirectiveName}" +
				$"{_separateInLine}" +
				$"({_tagHandle})" +
				$"{_separateInLine}" +
				$"{_tagPrefix}" +
				$"{Comment}";
		}

		public static class NodeTags
		{
			// Even though '!<!>' satisfies the regex, it's not a valid verbatim tag since verbatim tags are not
			// subject to tag resolution.
			private static readonly string _verbatimTag = $"!<{Characters.UriChar}{{1,{Characters.CharGroupLength}}}>";

			private static readonly string _shorthandTag =
				$"{_tagHandle}{Characters.TagChar}{{1,{Characters.CharGroupLength}}}";

			private static readonly string _nonSpecificTag = $"{Characters.Tag}";

			private static readonly string _tagProperty = $"({_verbatimTag}|{_shorthandTag}|{_nonSpecificTag})";

			private static readonly string _anchorName = $"{Characters.AnchorChar}{{1,{Characters.CharGroupLength}}}";

			private static readonly string _anchorProperty = $"{Characters.Anchor}({_anchorName})";

			public static readonly string TagAnchorProperties =
				$"^{_separateInLine}{_tagProperty}(?:{_separateInLine}{_anchorProperty})?{_separateInLine}";

			public static readonly string AnchorTagProperties =
				$"^{_separateInLine}{_anchorProperty}(?:{_separateInLine}{_tagProperty})?{_separateInLine}";
		}
	}
}
