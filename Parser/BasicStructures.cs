using System;
using Parser.TypeDefinitions;

namespace Parser
{
	public static class BasicStructures
	{
		public const int CharGroupLength = 100;

		// TODO: Define the break code by the file parsed.
		public static readonly string Break = Environment.NewLine;

		public static readonly string Spaces = $"{Characters.SPACE}{{1,{CharGroupLength}}}";

		private static readonly string _indent = $"{Characters.SPACE}{{0,{CharGroupLength}}}";

		private static readonly string _anchoredIndent = $"^{_indent}";

		private static readonly string _separateInLine = $"(?:^|[{Characters.SPACE}{Characters.TAB}]{{1,{CharGroupLength}}})";

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
			return $"{Break}" +
				   $"(?:{EmptyLine(c, useAnchoredIndent: false)})+";
		}

		// TODO: When writing the processor, one matter should be observed:
		// if line breaks within a block surround a more intended line, then folding doesn't apply to such breaks.
		public static string BreakAsSpace(string linePrefix = "") =>
			$"{Break}" +
			linePrefix + $"(?=.{{0,{CharGroupLength}}}[^ \t{Break}]{{1,{CharGroupLength}}}.{{0,{CharGroupLength}}})";

		public static string FlowFoldedTrimmedLine =
			$"(?:{_separateInLine})?" +
			TrimmedLine(BlockFlow.FlowIn) +
			LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false);

		public static string FlowFoldedLineWithBreakAsSpace =
			$"(?:{_separateInLine})?" +
			BreakAsSpace(linePrefix: LinePrefix(BlockFlow.FlowIn, useAnchoredIndent: false));

		#endregion

		public static readonly string CommentRegex =
			$"(?:{_separateInLine}(?:#[^{Break}]{{0,{CharGroupLength * CharGroupLength}}})?)?{Break}";

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
					return $"(?:{CommentRegex}" +
						   $"(?:{CommentRegex})*" +
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
	}
}