namespace Processor
{
	public static class Characters
	{
		#region Unicode Characters Codes

		#region C0ControlBlock

		public static readonly RegexPattern Tab = (RegexPattern) "\u0009";
		private const string _c0ControlBlockExceptTabLfCr = "\u0000-\u0008\u000B\u000C\u000E-\u001F";
		private const string _lf = "\u000A";
		private const string _cr = "\u000D";

		#endregion

		#region C1ControlBlock

		private const string _c1ControlBlockExceptNel = "\u0080-\u0084\u0086-\u009F";
		private const string _nel = "\u0085";

		#endregion

		public static readonly RegexPattern Space = (RegexPattern) "\u0020";
		private static readonly string _basicLatinSubset = $"{Space}-\u007E";
		private const string _del = "\u007F";
		private const string _latinSupplementToHangulJamo = "\u00A0-\uD7FF";
		private const string _byteOrderMark = "\uFEFF";
		private const string _surrogateBlock = "\uD800-\uDFFF";
		private const string _notChars = "\uFFFE\uFFFF";
		private const string _privateUseAreaToSpecialsBeginning = "\uE000-\uFFFD";
		// This is a workaround for using "[\U00010000-\U0010FFFF]" in regex.
		private const string _linearBSyllabaryToSupplementaryPrivateUseArea = "[\uD800-\uDBFF][\uDC00-\uDFFF]";
		private static readonly string _basicLatinToLast16BitChar =
			$"{Space}-\uD7FF{_privateUseAreaToSpecialsBeginning}{_notChars}";

		#endregion

		#region Indicator Characters

		public static readonly RegexPattern SequenceEntry = (RegexPattern) "\u002D";	// -
		public static readonly RegexPattern MappingKey = (RegexPattern) "\u003F";		// ?
		public static readonly RegexPattern MappingValue = (RegexPattern) "\u003A";		// :
		public static readonly RegexPattern CollectEntry = (RegexPattern) "\u002C";		// ,
		public static readonly RegexPattern SequenceStart = (RegexPattern) "\u005B";	// [
		public static readonly RegexPattern SequenceEnd = (RegexPattern) "\u005D";		// ]
		public static readonly RegexPattern MappingStart = (RegexPattern) "\u007B";		// {
		public static readonly RegexPattern MappingEnd = (RegexPattern) "\u007D";		// }
		public static readonly RegexPattern Comment = (RegexPattern) "\u0023";			// #
		public static readonly RegexPattern Anchor = (RegexPattern) "\u0026";			// &
		public static readonly RegexPattern Alias = (RegexPattern) "\u002A";			// *
		public static readonly RegexPattern Tag = (RegexPattern) "\u0021";				// !
		public static readonly RegexPattern Literal = (RegexPattern) "\u007C";			// |
		public static readonly RegexPattern Folded = (RegexPattern) "\u003E";			// >
		public static readonly RegexPattern SingleQuote = (RegexPattern) "\u0027";		// '
		public static readonly RegexPattern DoubleQuote = (RegexPattern) "\u0022";		// "
		public static readonly RegexPattern Directive = (RegexPattern) "\u0025";		// %
		public static readonly RegexPattern ReservedChar1 = (RegexPattern) "\u0040";	// @
		public static readonly RegexPattern ReservedChar2 = (RegexPattern) "\u0060";	// `

		public static readonly string FlowIndicators =
			$@"{CollectEntry}\{SequenceStart}\{SequenceEnd}{MappingStart}{MappingEnd}";

		public static readonly string CIndicators =
			$@"{SequenceEntry}{MappingKey}{MappingValue}{CollectEntry}\{SequenceStart}\{SequenceEnd}" +
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
			$"(?:{_escape}" +
			RegexPatternBuilder.BuildCharSet(
				_escapedNull,
				_escapedBell,
				_escapedBackspace,
				_escapedHorizontalTab,
				_escapedLineFeed,
				_escapedVerticalTab,
				_escapedFormFeed,
				_escapedCarriageReturn,
				_escapedEscape,
				_escapedSpace,
				_escapedDoubleQuote,
				_escapedSlash,
				_escapedBackslash,
				_escapedNextLine,
				_escapedNonBreakingSpace,
				_escapedLineSeparator,
				_escapedParagraphSeparator,
				_escaped8Bit,
				_escaped16Bit,
				_escaped32Bit
			) + ")";

		#endregion

		public const string VersionSeparator = ".";

		public static readonly string ForbiddenCharsRegex =
			RegexPatternBuilder.BuildCharSet(
				_c0ControlBlockExceptTabLfCr,
				_c1ControlBlockExceptNel,
				_del,
				_surrogateBlock,
				_notChars
			);

		public static readonly string PrintableChar =
			RegexPatternBuilder.BuildAlternation(
				RegexPatternBuilder.BuildCharSet(
					Tab,
					_lf,
					_cr,
					_nel,
					_basicLatinSubset,
					_latinSupplementToHangulJamo,
					_privateUseAreaToSpecialsBeginning
				),
				_linearBSyllabaryToSupplementaryPrivateUseArea
			);

		public static readonly RegexPattern JsonCompatibleChar =
			RegexPatternBuilder.BuildAlternation(
				RegexPatternBuilder.BuildCharSet(Tab, _basicLatinToLast16BitChar),
				_linearBSyllabaryToSupplementaryPrivateUseArea
			);

		public static readonly string FlowIndicatorsRegex = RegexPatternBuilder.BuildCharSet(
			CollectEntry,
			$@"\{SequenceStart}",
			$@"\{SequenceEnd}",
			MappingStart,
			MappingEnd
		);

		public const string DecimalDigits = "0-9";
		internal static readonly RegexPattern SWhites = Space + Tab;
		private const string _asciiLetters = "A-Za-z";
		private static readonly string _hexDigits = $"{DecimalDigits}A-Fa-f";

		internal static readonly string WordChar =
			RegexPatternBuilder.BuildCharSet(
				DecimalDigits,
				_asciiLetters,
				SequenceEntry
			);

		internal static readonly string UriChar =
			RegexPatternBuilder.BuildAlternation(
				"%" + RegexPatternBuilder.BuildCharSet(_hexDigits) + "{2}",
				WordChar,
				RegexPatternBuilder.BuildCharSet("#;\\/?:@&=+$,_.!~*'()\\[\\]”")
			);

		internal static readonly RegexPattern TagChar = RegexPatternBuilder.BuildExclusive(
			exclusiveChars: Tag + FlowIndicators,
			inclusiveChars: UriChar
		);

		internal static readonly RegexPattern NbChar = RegexPatternBuilder.BuildExclusive(
			exclusiveChars: _lf + _cr + _byteOrderMark,
			inclusiveChars: PrintableChar
		);

		internal static readonly RegexPattern NsChar = RegexPatternBuilder.BuildExclusive(
			exclusiveChars: SWhites,
			inclusiveChars: NbChar
		);

		internal static readonly RegexPattern AnchorChar = RegexPatternBuilder.BuildExclusive(
			exclusiveChars: FlowIndicators,
			inclusiveChars: NsChar
		);

		public const int CharGroupLength = 1000;
	}
}
