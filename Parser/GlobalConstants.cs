using System;
using Parser.TypeDefinitions;

namespace Parser
{
	public static class GlobalConstants
	{
		#region Unicode Characters Codes

		#region C0ControlBlock

		private const string C0ControlBlockExceptTabLfCr = "\u0000-\u0008\u000B\u000C\u000E-\u001F";
		private const string TAB = "\u0009";
		private const string LF = "\u000A";
		private const string CR = "\u000D";

		#endregion

		private const string SPACE = "\u0020";
		private const string BasicLatinSubset = "\u0020-\u007E";

		private const string DEL = "\u007F";

		#region C1ControlBlock

		private const string C1ControlBlockExceptNel = "\u0080-\u0084\u0086-\u009F";
		private const string NEL = "\u0085";

		#endregion

		private const string LatinSupplementToHangulJamo = "\u00A0-\uD7FF";

		private const string ByteOrderMark = "\uFEFF";
		private const string SurrogateBlock = "\uD800-\uDFFF\uFFFE\uFFFF";

		private const string PrivateUseAreaToSpecialsBeginning = "\uE000-\uFFFD";

		// This is a workaround for using "[\U00010000-\U0010FFFF]" in regex.
		private const string LinearBSyllabaryToSupplementaryPrivateUseAreaRegex = "[\uD800-\uDBFF][\uDC00-\uDFFF]";

		private const string BasicLatinToSupplementaryPrivateUseArea = "\u0020-\U0010FFFF";

		#endregion

		#region Indicator Characters

		public const string SequenceEntry = "\u002D";	// -
		public const string MappingKey = "\u003F";		// ?
		public const string MappingValue = "\u003A";	// :
		public const string CollectEntry = "\u002C";	// ,
		public const string SequenceStart = "\u005B";	// [
		public const string SequenceEnd = "\u005D";		// ]
		public const string MappingStart = "\u007B";	// {
		public const string MappingEnd = "\u007D";		// }
		public const string Comment = "\u0023";			// #
		public const string Anchor = "\u0026";			// &
		public const string Alias = "\u002A";			// *
		public const string Tag = "\u0021";				// !
		public const string Literal = "\u007C";			// |
		public const string Folded = "\u003E";			// >
		public const string SingleQuote = "\u0027";		// "
		public const string DoubleQuote = "\u0022";		// "
		public const string Directive = "\u0025";		// %
		public const string ReservedChar1 = "\u0040";	// @
		public const string ReservedChar2 = "\u0060";	// `

		#endregion

		public const int CharSequenceLength = 100;

		public static readonly string Break = $"({CR}{LF}?|{LF})";

		public static readonly string Spaces = $"{SPACE}{{1,{CharSequenceLength}}}";

		public static readonly string Indent = $"^{Spaces}";

		public static readonly string SeparateInLine = $"[{SPACE}{TAB}]{{1,{CharSequenceLength}}}";

		public static string LinePrefix(BlockFlowInOut c)
		{
			switch (c)
			{
				case BlockFlowInOut.BlockOut:
				case BlockFlowInOut.BlockIn:
					return Indent;
				case BlockFlowInOut.FlowOut:
				case BlockFlowInOut.FlowIn:
					return $"{Indent}({SeparateInLine})?";
				default:
					throw new ArgumentOutOfRangeException(nameof(c), c, $"Unknown {nameof(BlockFlowInOut)} item {c}.");
			}
		}

		public static string EmptyLine(BlockFlowInOut c)
		{
			return $"{LinePrefix(c)}{Break}";
		}

		public static string TrimmedLine(BlockFlowInOut c)
		{
			return $"{Break}({EmptyLine(c)})+";
		}

		public static string FoldedLine(BlockFlowInOut c)
		{
			return $"{TrimmedLine(c)}|{Break}";
		}

		public static readonly string CommentRegex =
			$"(?:{Spaces}#.{{1,{CharSequenceLength * CharSequenceLength}}})?$";

		public static readonly string ForbiddenCharsRegex =
			$"[{C0ControlBlockExceptTabLfCr + C1ControlBlockExceptNel + DEL + SurrogateBlock}]";

		public static readonly string PrintableCharsRegex =
			@$"[{TAB + LF + CR + NEL + 
				 BasicLatinSubset + 
				 LatinSupplementToHangulJamo + 
				 PrivateUseAreaToSpecialsBeginning}]" + 
				"|" +
				 $"{LinearBSyllabaryToSupplementaryPrivateUseAreaRegex}";

		public static readonly string JsonCompatibleRegex = $"[{TAB + BasicLatinToSupplementaryPrivateUseArea}]";

		public static readonly string FlowIndicatorsRegex =
			$"[{CollectEntry + SequenceStart + SequenceEnd + MappingStart + MappingEnd}]";

		public static readonly string WhiteSpaceCharsRegex = $"[{SPACE + TAB}]";
	}
}
