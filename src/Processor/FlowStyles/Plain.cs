using System;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.FlowStyles
{
	public static class Plain
	{
		private static readonly RegexPattern _nsPlainSafeOut = Characters.NsChar;

		private static readonly RegexPattern _nsPlainSafeIn = RegexPatternBuilder.BuildExclusive(
			exclusiveChars: Characters.FlowIndicators,
			inclusiveChars: Characters.NsChar
		);

		private static RegexPattern getNsPlainSafe(Context context) => context switch
		{
			Context.FlowOut or Context.BlockKey => _nsPlainSafeOut,
			Context.FlowIn or Context.FlowKey => _nsPlainSafeIn,
			_ => throw new ArgumentOutOfRangeException(nameof(context), context, null),
		};

		private static RegexPattern getNsPlainFirst(Context context) =>
			RegexPatternBuilder.BuildAlternation(
				RegexPatternBuilder.BuildExclusive(
					exclusiveChars: Characters.CIndicators,
					inclusiveChars: Characters.NsChar
				),
				RegexPatternBuilder.BuildLookAhead(
					beforeChars: RegexPatternBuilder.BuildCharSet(
						Characters.MappingKey,
						Characters.MappingValue,
						Characters.SequenceEntry
					),
					lookAheadExpression: getNsPlainSafe(context)
				)
			);

		private static RegexPattern getNsPlainChar(Context context) =>
			RegexPatternBuilder.BuildAlternation(
				RegexPatternBuilder.BuildExclusive(
					exclusiveChars: Characters.MappingValue + Characters.Comment,
					inclusiveChars: getNsPlainSafe(context)
				),
				RegexPatternBuilder.BuildLookBehind(
					lookBehindExpression: Characters.NsChar,
					afterChars: Characters.Comment
				),
				RegexPatternBuilder.BuildLookAhead(
					beforeChars: Characters.MappingValue,
					lookAheadExpression: getNsPlainSafe(context)
				)
			);

		private static RegexPattern getNbNsPlainInLine(Context context) =>
			(
				RegexPatternBuilder.BuildCharSet(Characters.SWhites).WithLimitingRepetition() +
				getNsPlainChar(context)
			).WithLimitingRepetition();

		private static RegexPattern getNsPlainOneLine(Context context) =>
			(getNsPlainFirst(context) + getNbNsPlainInLine(context)).WithAnchorAtBeginning();

		public static RegexPattern GetPatternFor(Context context) => getNsPlainOneLine(context);

		public static class NextLine
		{
			private static RegexPattern getPlainNextLine(Context context) =>
				((getNsPlainChar(context) + getNbNsPlainInLine(context)).AsCapturingGroup() + BasicStructures.Break)
					.WithAnchorAtBeginning()
					.WithAnchorAtEnd();

			public static RegexPattern GetPatternFor(Context context) => context switch
			{
				Context.FlowIn or Context.FlowOut => getPlainNextLine(context),
				_ => throw new ArgumentOutOfRangeException(
						nameof(context),
						context,
						$"Only {Context.FlowIn} and {Context.FlowOut} are supported."
					),
			};
		}
	}
}
