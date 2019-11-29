namespace Parser
{
	public static class GlobalConstants
	{
		public const int CharSequenceLength = 100;

		public static readonly string CommentRegex =
			$"(?: {{1,{CharSequenceLength}}}#.{{1,{CharSequenceLength * CharSequenceLength}}})?$";
	}
}