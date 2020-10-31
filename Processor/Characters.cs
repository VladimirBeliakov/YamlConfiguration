namespace Processor
{
	public static class Characters
	{
		#region Unicode Characters Codes

		#region C0ControlBlock

		public const string TAB = "\u0009";
		private const string _c0ControlBlockExceptTabLfCr = "\u0000-\u0008\u000B\u000C\u000E-\u001F";
		private const string _lf = "\u000A";
		private const string _cr = "\u000D";

		#endregion

		#region C1ControlBlock

		private const string _c1ControlBlockExceptNel = "\u0080-\u0084\u0086-\u009F";
		private const string _nel = "\u0085";

		#endregion

		public const string SPACE = "\u0020";
		private static readonly string _basicLatinSubset = $"{SPACE}-\u007E";
		private const string _del = "\u007F";
		private const string _latinSupplementToHangulJamo = "\u00A0-\uD7FF";
		private const string _byteOrderMark = "\uFEFF";
		private const string _surrogateBlock = "\uD800-\uDFFF";
		private const string _notChars = "\uFFFE\uFFFF";
		private const string _privateUseAreaToSpecialsBeginning = "\uE000-\uFFFD";
		// This is a workaround for using "[\U00010000-\U0010FFFF]" in regex.
		private const string _linearBSyllabaryToSupplementaryPrivateUseArea = "[\uD800-\uDBFF][\uDC00-\uDFFF]";
		private static readonly string _basicLatinToLast16BitChar =
			$"{SPACE}-\uD7FF{_privateUseAreaToSpecialsBeginning}{_notChars}";

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
		public const string SingleQuote = "\u0027";		// '
		public const string DoubleQuote = "\u0022";		// "
		public const string Directive = "\u0025";		// %
		public const string ReservedChar1 = "\u0040";	// @
		public const string ReservedChar2 = "\u0060";	// `

		public static readonly string FlowIndicators =
			$@"{CollectEntry}\{SequenceStart}\{SequenceEnd}{MappingStart}{MappingEnd}";

		public static readonly string CIndicators =
			$"{SequenceEntry}{MappingKey}{MappingValue}{CollectEntry}{SequenceStart}{SequenceEnd}" +
			$"{MappingStart}{MappingEnd}{Comment}{Anchor}{Alias}{Tag}{Literal}{Folded}{SingleQuote}{DoubleQuote}" +
			$"{Directive}{ReservedChar1}{ReservedChar2}";

		#endregion

		#region Escape Sequences

		private const string _escape = "\\\\";
		private const string _escapedNull = "0";
		private const string _escapedBell = "a";
		private const string _escapedBackspace = "b";
		private const string _escapedHorizontalTab = "t";
		private const string _escapedLineFeed = "n";
		private const string _escapedVerticalTab = "v";
		private const string _escapedFormFeed = "f";
		private const string _escapedCarriageReturn = "r";
		private const string _escapedEscape = "e";
		private const string _escapedSpace = " ";
		private const string _escapedDoubleQuote = "\"";
		private const string _escapedSlash = "/";
		private const string _escapedBackslash = "\\\\";
		private const string _escapedNextLine = "N";
		private const string _escapedNonBreakingSpace = "\u00A0";
		private const string _escapedLineSeparator = "L";
		private const string _escapedParagraphSeparator = "P";
		private const string _escaped8Bit = "x";
		private const string _escaped16Bit = "u";
		private const string _escaped32Bit = "U";

		public static readonly string EscapedChar =
			$"(?:{_escape}[" +
			$"{_escapedNull}" +
			$"{_escapedBell}" +
			$"{_escapedBackspace}" +
			$"{_escapedHorizontalTab}" +
			$"{_escapedLineFeed}" +
			$"{_escapedVerticalTab}" +
			$"{_escapedFormFeed}" +
			$"{_escapedCarriageReturn}" +
			$"{_escapedEscape}" +
			$"{_escapedSpace}" +
			$"{_escapedDoubleQuote}" +
			$"{_escapedSlash}" +
			$"{_escapedBackslash}" +
			$"{_escapedNextLine}" +
			$"{_escapedNonBreakingSpace}" +
			$"{_escapedLineSeparator}" +
			$"{_escapedParagraphSeparator}" +
			$"{_escaped8Bit}" +
			$"{_escaped16Bit}" +
			$"{_escaped32Bit}" +
			"])";

		#endregion

		public const string VersionSeparator = ".";

		public static readonly string ForbiddenCharsRegex =
			$"[{_c0ControlBlockExceptTabLfCr + _c1ControlBlockExceptNel + _del + _surrogateBlock + _notChars}]";

		public static readonly string PrintableChar =
			$@"(?:[{TAB + _lf + _cr + _nel +
				 _basicLatinSubset +
				 _latinSupplementToHangulJamo +
				 _privateUseAreaToSpecialsBeginning}]" +
				"|" +
				 $"{_linearBSyllabaryToSupplementaryPrivateUseArea})";

		public static readonly string JsonCompatibleChar =
			$"(?:[{TAB + _basicLatinToLast16BitChar}]" +
			"|" +
			$"{_linearBSyllabaryToSupplementaryPrivateUseArea})";

		public static readonly string FlowIndicatorsRegex =
			$"[{CollectEntry + SequenceStart + SequenceEnd + MappingStart + MappingEnd}]";

		public const string DecimalDigits = "0-9";
		internal static readonly string WhiteSpaceChars= $"{SPACE + TAB}";
		private const string _asciiLetters = "A-Za-z";
		private static readonly string _hexDigits = $"{DecimalDigits}A-Fa-f";

		internal static readonly string WordChar = $"[{DecimalDigits}{_asciiLetters}-]";
		internal static readonly string UriChar = $"(?:%[{_hexDigits}]{{2}}|{WordChar}|[#;\\/?:@&=+$,_.!~*'()\\[\\]”])";

		internal static readonly string TagChar = RegexPatternBuilder.BuildWithExclusiveChars(
			exclusiveChars: Tag + FlowIndicators,
			inclusiveChars: UriChar
		);

		internal static readonly string NbChar = RegexPatternBuilder.BuildWithExclusiveChars(
			exclusiveChars: _lf + _cr + _byteOrderMark,
			inclusiveChars: PrintableChar
		);

		internal static readonly string NsChar = RegexPatternBuilder.BuildWithExclusiveChars(
			exclusiveChars: _lf + _cr + _byteOrderMark + WhiteSpaceChars,
			inclusiveChars: PrintableChar
		);

		internal static readonly string AnchorChar = RegexPatternBuilder.BuildWithExclusiveChars(
			exclusiveChars: FlowIndicators,
			inclusiveChars: NsChar
		);

		public const int CharGroupLength = 1000;
	}
}
