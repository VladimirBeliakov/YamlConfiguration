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
		private const string _basicLatinSubset = "\u0020-\u007E";
		private const string _del = "\u007F";
		private const string _latinSupplementToHangulJamo = "\u00A0-\uD7FF";
		private const string _byteOrderMark = "\uFEFF";
		private const string _surrogateBlock = "\uD800-\uDFFF\uFFFE\uFFFF";
		private const string _privateUseAreaToSpecialsBeginning = "\uE000-\uFFFD";
		// This is a workaround for using "[\U00010000-\U0010FFFF]" in regex.
		private const string _linearBSyllabaryToSupplementaryPrivateUseArea = "[\uD800-\uDBFF][\uDC00-\uDFFF]";
		private const string _basicLatinToSupplementaryPrivateUseArea = "\u0020-\U0010FFFF";

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
		private const string _flowIndicators = ",\\[\\]{}";

		#endregion

		public const string VersionSeparator = ".";

		public static readonly string ForbiddenCharsRegex =
			$"[{_c0ControlBlockExceptTabLfCr + _c1ControlBlockExceptNel + _del + _surrogateBlock}]";

		public static readonly string PrintableChar =
			$@"(?:[{TAB + _lf + _cr + _nel +
				 _basicLatinSubset +
				 _latinSupplementToHangulJamo +
				 _privateUseAreaToSpecialsBeginning}]" +
				"|" +
				 $"{_linearBSyllabaryToSupplementaryPrivateUseArea})";

		public static readonly string JsonCompatibleRegex = $"[{TAB + _basicLatinToSupplementaryPrivateUseArea}]";

		public static readonly string FlowIndicatorsRegex =
			$"[{CollectEntry + SequenceStart + SequenceEnd + MappingStart + MappingEnd}]";

		public const string DecimalDigits = "0-9";
		private static readonly string _whiteSpaceChars= $"{SPACE + TAB}";
		private const string _asciiLetters = "A-Za-z";
		private static readonly string _hexDigits = $"{DecimalDigits}A-Fa-f";

		internal static readonly string WordChar = $"[{DecimalDigits}{_asciiLetters}-]";
		internal static readonly string UriChar = $"(?:%[{_hexDigits}]{{2}}|{WordChar}|[#;\\/?:@&=+$,_.!~*'()\\[\\]”])";
		// TODO: When writing negative TagAnchor tests change this to (?![{Tag}{_flowIndicators}]){UriChar} to check if the tests fall with an error
		internal static readonly string TagChar = $"(?:(?![{Tag}{_flowIndicators}]){UriChar})";

		internal static readonly string NonBreakChar =
			$"(?:(?![{_lf + _cr + _byteOrderMark}]){PrintableChar})";

		internal static readonly string NonSpaceChar =
			$"(?:(?![{_lf + _cr + _byteOrderMark + _whiteSpaceChars}]){PrintableChar})";

		internal static readonly string AnchorChar = $"(?:(?![{_flowIndicators}]){NonSpaceChar})";

		public const int CharGroupLength = 100;
	}
}
