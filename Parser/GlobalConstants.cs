using System.Linq;

namespace Parser
{
	public static class GlobalConstants
	{
		#region Unicode Characters Codes

		private const string C0ControlBlock = "\u0000-\u001F";
		private const char TAB = '\u0009';
		private const char LF = '\u000A';
		private const char CR = '\u000D';

		private const string BasicLatinSubset = "\u0020-\u007E";

		private const char DEL = '\u007F';

		private const string C1ControlBlock = "\u0080-\u009F";
		private const char NEL = '\u0085';

		private const string LatinSupplementToHangulJamo = "\u00A0-\uD7FF";

		private const string SurrogateBlock = "\uD800-\uDFFF\uFFFE\uFFFF";

		private const string PrivateUseAreaToSpecialsBeginning = "\uE000-\uFFFD";

		private const string LinearBSyllabaryToSupplementaryPrivateUseArea = "\u10000-\u10FFFF";

		#endregion

		public const int CharSequenceLength = 100;

		public static readonly string CommentRegex =
			$"(?: {{1,{CharSequenceLength}}}#.{{1,{CharSequenceLength * CharSequenceLength}}})?$";

		public static readonly string SpacesRegex = $" {{1,{CharSequenceLength}}}";

		public static readonly string ForbiddenChars =
			C0ControlBlock.Except(new[] { TAB, LF, CR })
				.Concat(C1ControlBlock.Except(new[] { NEL }))
				.Concat(new[] { DEL })
				.Concat(SurrogateBlock)
				.ToString();

		public static readonly string PrintableChars =
			new[] { TAB, LF, CR, NEL }
				.Concat(BasicLatinSubset)
				.Concat(LatinSupplementToHangulJamo)
				.Concat(PrivateUseAreaToSpecialsBeginning)
				.Concat(LinearBSyllabaryToSupplementaryPrivateUseArea)
				.ToString();
	}
}