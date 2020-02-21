using System.Linq;

namespace Parser
{
	public static class GlobalConstants
	{
		#region Unicode Characters Codes

		#region C0ControlBlock

		private const string C0ControlBlockExceptTabLfCr = "\u0000-\u0008\u000B\u000C\u000E-\u001F";
		private const char TAB = '\u0009';
		private const char LF = '\u000A';
		private const char CR = '\u000D';

		#endregion

		private const string BasicLatinSubset = "\u0020-\u007E";

		private const char DEL = '\u007F';

		private const string C1ControlBlockExceptNel = "\u0080-\u0084\u0086-\u009F";
		private const char NEL = '\u0085';

		private const string LatinSupplementToHangulJamo = "\u00A0-\uD7FF";

		private const string SurrogateBlock = "\uD800-\uDFFF\uFFFE\uFFFF";

		private const string PrivateUseAreaToSpecialsBeginning = "\uE000-\uFFFD";

		private const string LinearBSyllabaryToSupplementaryPrivateUseArea = "\U00010000-\U0010FFFF";

		private const string BasicLatinToSupplementaryPrivateUseArea = "\u0020-\U0010FFFF";

		#endregion

		public const int CharSequenceLength = 100;

		public static readonly string CommentRegex =
			$"(?: {{1,{CharSequenceLength}}}#.{{1,{CharSequenceLength * CharSequenceLength}}})?$";

		public static readonly string SpacesRegex = $" {{1,{CharSequenceLength}}}";

		public static readonly string ForbiddenCharsRegex =
			C0ControlBlockExceptTabLfCr + C1ControlBlockExceptNel + DEL + SurrogateBlock;

		public static readonly string PrintableChars =
			TAB + LF + CR + NEL +
			BasicLatinSubset +
			LatinSupplementToHangulJamo +
			PrivateUseAreaToSpecialsBeginning +
			LinearBSyllabaryToSupplementaryPrivateUseArea;

		public static readonly string JsonCompatible = TAB + BasicLatinToSupplementaryPrivateUseArea;
	}
}