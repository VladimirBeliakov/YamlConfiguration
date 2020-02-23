using System.Linq;

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

		public const int CharSequenceLength = 100;

		public static readonly string CommentRegex =
			$"(?: {{1,{CharSequenceLength}}}#.{{1,{CharSequenceLength * CharSequenceLength}}})?$";

		public static readonly string SpacesRegex = $" {{1,{CharSequenceLength}}}";

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
	}
}