using System;
using System.Linq;
using System.Text.RegularExpressions;
using Processor.TypeDefinitions;

namespace Processor.FlowStyles
{
	public class DoubleQuotedStyles
	{
		private static readonly string _jsonWithoutSlashAndDoubleQuote =
			$"(?:(?![\\\\\"]){Characters.JsonCompatibleChar})";

		private static readonly string _nbDoubleChar =
			$"(?:{Characters.EscapedChar}|{_jsonWithoutSlashAndDoubleQuote})";

		private static readonly string _nbDoubleOneLine =
			$"\"({_nbDoubleChar}{{0,{Characters.CharGroupLength}}})\"";

		private static readonly Regex _oneLineRegex = new Regex(_nbDoubleOneLine, RegexOptions.Compiled);

		// case BlockFlow.BlockKey
		// case BlockFlow.FlowKey
		public static bool TryProcessOneLine(string value, out string extractedValue)
		{
			extractedValue = null;

			var match = _oneLineRegex.Match(value);

			if (match.Success)
			{
				extractedValue = match.Groups[1].Captures[0].Value;
				return true;
			}

			return false;
		}

		// case BlockFlow.FlowIn
		// case BlockFlow.FlowOut
		public class MultiLine
		{
			private static readonly string _foldedLineSequenceBeginning =
				$"(?:{BasicStructures.SeparateInLine}?{BasicStructures.Break})";

			private static readonly string _doubleEscapeSequenceBeginning =
				$"(?:({BasicStructures.SeparateInLine}?)\\\\{BasicStructures.Break})";

			private static readonly string _nonSpaceDoubleChar =
				$"(?:(?![{Characters.WhiteSpaceChars}]){_nbDoubleChar})";

			private static readonly string _nbNsDoubleInLine =
				$"(?:[{Characters.WhiteSpaceChars}]{{0,{Characters.CharGroupLength}}}" +
				$"{_nonSpaceDoubleChar}){{0,{Characters.CharGroupLength}}}";

			private static readonly string _nbDoubleFirstLine = $"^\"({_nbNsDoubleInLine})";

			private static readonly string _emptyLineWithoutBreak = BasicStructures.LinePrefix(BlockFlow.FlowIn);

			private static readonly string _nonEmptyLine =
				BasicStructures.LinePrefix(BlockFlow.FlowIn) + $"({_nonSpaceDoubleChar + _nbNsDoubleInLine})";

			private static readonly Regex _firstLineWithoutEscapedBreakRegex = new Regex(
				_nbDoubleFirstLine + _foldedLineSequenceBeginning,
				RegexOptions.Compiled
			);

			private static readonly Regex _firstLineWithEscapedBreakRegex = new Regex(
				_nbDoubleFirstLine + _doubleEscapeSequenceBeginning,
				RegexOptions.Compiled
			);

			private static readonly Regex _emptyLineRegex = new Regex(
				_emptyLineWithoutBreak + BasicStructures.Break,
				RegexOptions.Compiled
			);

			private static readonly Regex _nonEmptyLineWithoutEscapedBreakRegex =
				new Regex(_nonEmptyLine + _foldedLineSequenceBeginning, RegexOptions.Compiled);

			private static readonly Regex _nonEmptyLineWithEscapedBreakRegex =
				new Regex(_nonEmptyLine + _doubleEscapeSequenceBeginning, RegexOptions.Compiled);

			private static readonly Regex _lastEmptyLineRegex = new Regex(
				_emptyLineWithoutBreak + "\"$",
				RegexOptions.Compiled
			);
			private static readonly Regex _lastNonEmptyLineRegex = new Regex(
				_nonEmptyLine + $"({BasicStructures.SeparateInLine}?)" + "\"$",
				RegexOptions.Compiled
			);

			private bool _wasFirstLineProcessed;
			private bool _wasLastLineProcessed;

			public ProcessedLineResult ProcessFirstLine(string value)
			{
				if (_wasFirstLineProcessed)
					throw new InvalidOperationException("First line was already processed.");

				_wasFirstLineProcessed = true;

				var match = _firstLineWithoutEscapedBreakRegex.Match(value);

				if (match.Success)
				{
					var doubleInLine = match.Groups[1].Captures[0].Value;

					return ProcessedLineResult.First(extractedValue: doubleInLine);
				}

				match = _firstLineWithEscapedBreakRegex.Match(value);

				if (match.Success)
				{
					var doubleInLine = match.Groups[1].Captures[0].Value;
					var trailingWhiteSpaceChars = match.Groups[2].Captures[0].Value;

					return ProcessedLineResult.First(extractedValue: doubleInLine + trailingWhiteSpaceChars);
				}

				return ProcessedLineResult.Invalid();
			}

			// TODO: Allow for an escaped space in the beginning of the line. It makes all the leading white chars not
			// TODO: to be folded
			public ProcessedLineResult ProcessNextLine(string value)
			{
				validateNextLineProcessing();

				var match = _emptyLineRegex.Match(value);

				if (match.Success)
					return ProcessedLineResult.Empty();

				match = _nonEmptyLineWithoutEscapedBreakRegex.Match(value);

				if (match.Success)
				{
					var doubleInLine = match.Groups[1].Captures[0].Value;

					return ProcessedLineResult.NotEmpty(extractedValue: doubleInLine);
				}

				match = _nonEmptyLineWithEscapedBreakRegex.Match(value);

				if (match.Success)
				{
					var doubleInLine = match.Groups[1].Captures[0].Value;
					var trailingWhiteSpaceChars = match.Groups[2].Captures[0].Value;

					return ProcessedLineResult.NotEmpty(extractedValue: doubleInLine + trailingWhiteSpaceChars);
				}

				match = _lastNonEmptyLineRegex.Match(value);

				if (match.Success)
				{
					_wasLastLineProcessed = true;

					var doubleInLine = match.Groups[1].Captures[0].Value;
					var trailingWhiteSpaceChars = match.Groups[2].Captures[0].Value;

					return ProcessedLineResult.LastNotEmpty(extractedValue: doubleInLine + trailingWhiteSpaceChars);
				}

				match = _lastEmptyLineRegex.Match(value);

				if (match.Success)
				{
					_wasLastLineProcessed = true;

					return ProcessedLineResult.LastEmpty();
				}

				return ProcessedLineResult.Invalid();
			}

			private void validateNextLineProcessing()
			{
				if (!_wasFirstLineProcessed)
					throw new InvalidOperationException(
						"First line must be processed before processing the next line."
					);

				if (_wasLastLineProcessed)
					throw new InvalidOperationException(
						"Can't process the next line because the last line was already processed."
					);
			}
		}
	}
}
