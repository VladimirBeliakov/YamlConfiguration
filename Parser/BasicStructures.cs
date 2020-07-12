using System;
using Parser.TypeDefinitions;

namespace Parser
{
	public static class BasicStructures
	{
		// TODO: Define the break code by the file parsed.
		public static readonly string Break = Environment.NewLine;

		public static readonly string Spaces = $"{Characters.SPACE}{{1,{Characters.CharGroupLength}}}";

		private static readonly string _indent = $"{Characters.SPACE}{{0,{Characters.CharGroupLength}}}";

		private static readonly string _anchoredIndent = $"^{_indent}";

		public static readonly string SeparateInLine =
			$"(?:^|[{Characters.SPACE}{Characters.TAB}]{{1,{Characters.CharGroupLength}}})";

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
					return $"{indent}{SeparateInLine}?";
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
			return $"{Break}" +
				   $"(?:{EmptyLine(c, useAnchoredIndent: false)})+";
		}

		// TODO: When writing the processor, one matter should be observed:
		// if line breaks within a block surround a more intended line, then folding doesn't apply to such breaks.
		public static string BreakAsSpace(string linePrefix = "") =>
			Break +
			linePrefix +
			$"(?=.{{0,{Characters.CharGroupLength}}}[^ \t{Break}]{{1,{Characters.CharGroupLength}}}.{{0,{Characters.CharGroupLength}}})";

		public static string FlowFoldedTrimmedLine =
			$"(?:{SeparateInLine})?" +
			TrimmedLine(BlockFlow.FlowIn) +
			LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false);

		public static string FlowFoldedLineWithBreakAsSpace =
			$"(?:{SeparateInLine})?" +
			BreakAsSpace(linePrefix: LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false));

		#endregion

		public static readonly string Comment =
			$"(?:{SeparateInLine}(?:#[^{Break}]{{0,{Characters.CharGroupLength * Characters.CharGroupLength}}})?)?{Break}";

		// TODO: Move the logic to a higher level.
		public static string SeparateLines(BlockFlow c)
		{
			throw new NotSupportedException("This regex is for informational purposes only. Will be deleted later.");
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
	}
}