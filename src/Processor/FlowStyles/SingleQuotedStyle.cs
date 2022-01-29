using System;
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

		private static readonly RegexPattern _nbSingleInLine =
			(
				Characters.SingleQuote +
				_nbSingleChar.WithLimitingRepetition().AsCapturingGroup() +
				Characters.SingleQuote
			).WithAnchorAtBeginning();

		public static RegexPattern GetInLinePatternFor(Context context) => context switch
		{
			Context.BlockKey or Context.FlowKey => _nbSingleInLine,
			_ => throw new ArgumentOutOfRangeException(
					nameof(context),
					context,
					$"Only {Context.BlockKey} and {Context.FlowKey} are allowed."
				),
		};

		public class MultiLine
		{
			private static readonly RegexPattern _whites =
				RegexPatternBuilder.BuildCharSet(Characters.SWhites).WithLimitingRepetition();

			private static readonly RegexPattern _closingSingleQuote =
				(_whites.AsCapturingGroup() + Characters.SingleQuote).AsCapturingGroup().AsOptional();

			private static readonly RegexPattern _nsSingleChar =
				RegexPatternBuilder.BuildExclusive(
					exclusiveChars: Characters.SWhites,
					inclusiveChars: _nbSingleChar
				);

			private static readonly RegexPattern _singleInLine = (_whites + _nsSingleChar).WithLimitingRepetition();

			private static readonly RegexPattern _singleInFirstLine =
				(Characters.SingleQuote + _singleInLine.AsCapturingGroup() + _closingSingleQuote)
					.WithAnchorAtBeginning();

			private static readonly RegexPattern _singleNextLine =
				((_nsSingleChar + _singleInLine).AsCapturingGroup() + _closingSingleQuote).WithAnchorAtBeginning();

			public static RegexPattern GetFirstLinePatternFor(Context context) =>
				context switch
				{
					Context.FlowIn or Context.FlowOut => _singleInFirstLine,
					_ => throw new ArgumentOutOfRangeException(
							nameof(context),
							context,
							$"Only {Context.FlowIn} and {Context.FlowOut} are allowed."
						),
				};

			public static RegexPattern GetNextLinePatternFor(Context context) =>
				context switch
				{
					Context.FlowIn or Context.FlowOut => _singleNextLine,
					_ => throw new ArgumentOutOfRangeException(
							nameof(context),
							context,
							$"Only {Context.FlowIn} and {Context.FlowOut} are allowed."
						),
				};
		}
	}
}
