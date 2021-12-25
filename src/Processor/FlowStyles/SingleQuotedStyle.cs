using System;
using System.Text.RegularExpressions;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.FlowStyles
{
	public static class SingleQuotedStyle
	{
		private static readonly RegexPattern _quotedQuote = Characters.SingleQuote + Characters.SingleQuote;

		private static readonly RegexPattern _jsonWithoutSingleQuote =
			RegexPatternBuilder.BuildExclusive(
				exclusiveChars: Characters.SingleQuote,
				inclusiveChars: Characters.JsonCompatibleChar
			);

		private static readonly RegexPattern _nbSingleChar =
			RegexPatternBuilder.BuildAlternation(_quotedQuote, _jsonWithoutSingleQuote);

		private static readonly RegexPattern _nbSingleFirstLine =
			(Characters.SingleQuote + _nbSingleChar.WithLimitingRepetition().AsCapturingGroup())
				.WithAnchorAtBeginning();

		private static readonly RegexPattern _nbSingleInLine = _nbSingleFirstLine + Characters.SingleQuote;

		public static RegexPattern GetPatternFor(Context context) => context switch
		{
			Context.BlockKey or Context.FlowKey => _nbSingleInLine,
			Context.FlowOut or Context.FlowIn => _nbSingleFirstLine + BasicStructures.Break,
			_ => throw new ArgumentOutOfRangeException(
				nameof(context),
				context,
				$"Only {Context.BlockKey}, {Context.FlowKey}, " +
				$"{Context.FlowIn}, and {Context.FlowOut} are allowed."
			),
		};

		// case BlockFlow.FlowIn
		// case BlockFlow.FlowOut
		public class MultiLine
		{
			private static readonly RegexPattern _nsSingleChar =
				RegexPatternBuilder.BuildExclusive(
					exclusiveChars: Characters.SWhites,
					inclusiveChars: _nbSingleChar
				);

			private static readonly RegexPattern _foldedLineSequenceBeginning =
				BasicStructures.SeparateInLine.AsOptional() + BasicStructures.Break;

			private static readonly RegexPattern _nbNsSingleInLine =
				(RegexPatternBuilder.BuildCharSet(Characters.SWhites).WithLimitingRepetition() + _nsSingleChar)
				.WithLimitingRepetition();

			private static readonly RegexPattern _nbNsSingleFirstLine =
				Characters.SingleQuote.WithAnchorAtBeginning() + _nbNsSingleInLine.AsCapturingGroup();

			private static readonly RegexPattern _emptyLineWithoutBreak = BasicStructures.LinePrefix(Context.FlowIn);

			private static readonly RegexPattern _nonEmptyLine =
				BasicStructures.LinePrefix(Context.FlowIn) + (_nsSingleChar + _nbNsSingleInLine).AsCapturingGroup();

			private static readonly Regex _firstLineBreakRegex = new(
				_nbNsSingleFirstLine + _foldedLineSequenceBeginning,
				RegexOptions.Compiled
			);

			private static readonly Regex _emptyLineRegex = new(
				_emptyLineWithoutBreak + BasicStructures.Break,
				RegexOptions.Compiled
			);

			private static readonly Regex _nonEmptyLineRegex =
				new(_nonEmptyLine + _foldedLineSequenceBeginning, RegexOptions.Compiled);

			private static readonly Regex _lastEmptyLineRegex = new(
				_emptyLineWithoutBreak + Characters.SingleQuote.WithAnchorAtEnd(),
				RegexOptions.Compiled
			);
			private static readonly Regex _lastNonEmptyLineRegex = new(
				_nonEmptyLine +
				BasicStructures.SeparateInLine.AsOptional().AsCapturingGroup() +
				Characters.SingleQuote.WithAnchorAtEnd(),
				RegexOptions.Compiled
			);

			private bool _wasFirstLineProcessed;
			private bool _wasLastLineProcessed;

			public ProcessedLineResult ProcessFirstLine(string value)
			{
				if (_wasFirstLineProcessed)
					throw new InvalidOperationException("First line was already processed.");

				_wasFirstLineProcessed = true;

				var match = _firstLineBreakRegex.Match(value);

				if (match.Success)
				{
					var singleInLine = match.Groups[1].Captures[0].Value;

					return ProcessedLineResult.First(extractedValue: singleInLine);
				}

				return ProcessedLineResult.Invalid();
			}

			public ProcessedLineResult ProcessNextLine(string value)
			{
				validateNextLineProcessing();

				var match = _emptyLineRegex.Match(value);

				if (match.Success)
					return ProcessedLineResult.Empty();

				match = _nonEmptyLineRegex.Match(value);

				if (match.Success)
				{
					var singleInLine = match.Groups[1].Captures[0].Value;

					return ProcessedLineResult.NotEmpty(extractedValue: singleInLine);
				}

				match = _lastNonEmptyLineRegex.Match(value);

				if (match.Success)
				{
					_wasLastLineProcessed = true;

					var singleInLine = match.Groups[1].Captures[0].Value;
					var trailingWhiteSpaceChars = match.Groups[2].Captures[0].Value;

					return ProcessedLineResult.LastNotEmpty(extractedValue: singleInLine + trailingWhiteSpaceChars);
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
