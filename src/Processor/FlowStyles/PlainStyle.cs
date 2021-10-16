using System;
using System.Text.RegularExpressions;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.FlowStyles
{
	public class PlainStyle
	{
		private static readonly RegexPattern _nsPlainSafeOut = Characters.NsChar;

		private static readonly RegexPattern _nsPlainSafeIn = RegexPatternBuilder.BuildExclusive(
			exclusiveChars: Characters.FlowIndicators,
			inclusiveChars: Characters.NsChar
		);

		private static RegexPattern getNsPlainSafe(Context context)
		{
			switch (context)
			{
				case Context.FlowOut:
				case Context.BlockKey:
					return _nsPlainSafeOut;
				case Context.FlowIn:
				case Context.FlowKey:
					return _nsPlainSafeIn;
				default:
					throw new ArgumentOutOfRangeException(nameof(context), context, null);
			}
		}

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
			(RegexPatternBuilder.BuildCharSet(Characters.SWhites).WithLimitingRepetition() + getNsPlainChar(context))
				.WithLimitingRepetition();

		private static RegexPattern getNsPlainOneLine(Context context) =>
			(getNsPlainFirst(context) + getNbNsPlainInLine(context)).WithAnchorAtBeginning();

		public static RegexPattern GetPatternFor(Context context)
		{
			switch (context)
			{
				case Context.BlockKey:
				case Context.FlowKey:
					return getNsPlainOneLine(context);
				default:
					throw new ArgumentOutOfRangeException(
						nameof(context),
						context,
						$"Only {Context.BlockKey} and {Context.FlowKey} are supported."
					);
			}
		}
	}
}