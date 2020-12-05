using System;
using System.Text.RegularExpressions;
using Processor.TypeDefinitions;

namespace Processor.FlowStyles
{
	public class PlainStyle
	{
		private static readonly RegexPattern _nsPlainSafeOut = Characters.NsChar;

		private static readonly RegexPattern _nsPlainSafeIn = RegexPatternBuilder.BuildExclusive(
			exclusiveChars: Characters.FlowIndicators,
			inclusiveChars: Characters.NsChar
		);

		private static RegexPattern getNsPlainSafe(BlockFlow blockFlow)
		{
			switch (blockFlow)
			{
				case BlockFlow.FlowOut:
				case BlockFlow.BlockKey:
					return _nsPlainSafeOut;
				case BlockFlow.FlowIn:
				case BlockFlow.FlowKey:
					return _nsPlainSafeIn;
				default:
					throw new ArgumentOutOfRangeException(nameof(blockFlow), blockFlow, null);
			}
		}

		private static RegexPattern getNsPlainFirst(BlockFlow blockFlow) =>
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
					lookAheadExpression: getNsPlainSafe(blockFlow)
				)
			);

		private static RegexPattern getNsPlainChar(BlockFlow blockFlow) =>
			RegexPatternBuilder.BuildAlternation(
				RegexPatternBuilder.BuildExclusive(
					exclusiveChars: Characters.MappingValue + Characters.Comment,
					inclusiveChars: getNsPlainSafe(blockFlow)
				),
				RegexPatternBuilder.BuildLookBehind(
					lookBehindExpression: Characters.NsChar,
					afterChars: Characters.Comment
				),
				RegexPatternBuilder.BuildLookAhead(
					beforeChars: Characters.MappingValue,
					lookAheadExpression: getNsPlainSafe(blockFlow)
				)
			);

		private static RegexPattern getNbNsPlainInLine(BlockFlow blockFlow) =>
			(RegexPatternBuilder.BuildCharSet(Characters.SWhites).WithLimitingRepetition() + getNsPlainChar(blockFlow))
				.WithLimitingRepetition();

		private static RegexPattern getNsPlainOneLine(BlockFlow blockFlow) =>
			(getNsPlainFirst(blockFlow) + getNbNsPlainInLine(blockFlow)).WithAnchorAtBeginning().WithAnchorAtEnd();

		private static readonly Regex _blockKeyOneLineRegex = new Regex(getNsPlainOneLine(BlockFlow.BlockKey));
		private static readonly Regex _flowKeyOneLineRegex = new Regex(getNsPlainOneLine(BlockFlow.FlowKey));

		// case BlockFlow.BlockKey
		// case BlockFlow.FlowKey
		public static bool IsOneLine(string value, BlockFlow blockFlow)
		{
			var regex = blockFlow switch
			{
				BlockFlow.BlockKey => _blockKeyOneLineRegex,
				BlockFlow.FlowKey => _flowKeyOneLineRegex,
				_ => throw new ArgumentOutOfRangeException(
					nameof(blockFlow),
					blockFlow,
					$"Only {BlockFlow.BlockKey} and {BlockFlow.FlowKey} can be processed."
				)
			};

			return regex.Match(value).Success;
		}
	}
}