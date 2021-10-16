using System;
using System.Text.RegularExpressions;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.FlowStyles
{
	public class DoubleQuotedStyle
	{
		private static readonly RegexPattern _jsonWithoutSlashAndDoubleQuote =
			RegexPatternBuilder.BuildExclusive(
					exclusiveChars: Characters.EscapedBackslash + Characters.DoubleQuote,
					inclusiveChars: Characters.JsonCompatibleChar
				);

		private static readonly RegexPattern _nbDoubleChar =
			RegexPatternBuilder.BuildAlternation(Characters.EscapedChar, _jsonWithoutSlashAndDoubleQuote);

		private static readonly string _nbDoubleOneLine =
			Characters.DoubleQuote + _nbDoubleChar.WithLimitingRepetition().AsCapturingGroup() + Characters.DoubleQuote;

		private static readonly Regex _oneLineRegex = new Regex(_nbDoubleOneLine, RegexOptions.Compiled);

		// case BlockFlow.BlockKey
		// case BlockFlow.FlowKey
		public static bool TryProcessOneLine(string value, out string? extractedValue)
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
			private static readonly RegexPattern _foldedLineSequenceBeginning =
				BasicStructures.SeparateInLine.AsOptional() + BasicStructures.Break;

			private static readonly RegexPattern _doubleEscapeSequenceBeginning =
				BasicStructures.SeparateInLine.AsOptional().AsCapturingGroup() +
				Characters.EscapedBackslash + BasicStructures.Break;

			private static readonly RegexPattern _nonSpaceDoubleChar =
				RegexPatternBuilder.BuildExclusive(
						exclusiveChars: Characters.SWhites,
						inclusiveChars: _nbDoubleChar
					);

			private static readonly RegexPattern _nbNsDoubleInLine =
				(
					RegexPatternBuilder.BuildCharSet(Characters.SWhites).WithLimitingRepetition() +
					_nonSpaceDoubleChar
				).WithLimitingRepetition();

			private static readonly RegexPattern _nbDoubleFirstLine =
					(Characters.DoubleQuote + _nbNsDoubleInLine.AsCapturingGroup()).WithAnchorAtBeginning();

			private static readonly RegexPattern _emptyLineWithoutBreak = BasicStructures.LinePrefix(Context.FlowIn);

			private static readonly RegexPattern _nonEmptyLine = BasicStructures.LinePrefix(Context.FlowIn) +
														   (_nonSpaceDoubleChar + _nbNsDoubleInLine).AsCapturingGroup();

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
				(_emptyLineWithoutBreak + Characters.DoubleQuote).WithAnchorAtEnd(),
				RegexOptions.Compiled
			);
			private static readonly Regex _lastNonEmptyLineRegex = new Regex(
				(
					_nonEmptyLine +
					BasicStructures.SeparateInLine.AsOptional().AsCapturingGroup() +
					Characters.DoubleQuote
				).WithAnchorAtEnd(),
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
