namespace Processor
{
	internal class RegexPattern
	{
		private readonly string _regexValue;

		public RegexPattern(string regexValue)
		{
			_regexValue = regexValue;
		}

		public override string ToString() => _regexValue;

		public static implicit operator string(RegexPattern pattern) => pattern._regexValue;

		public static explicit operator RegexPattern(string rawValue) => new RegexPattern(rawValue);

		public static RegexPattern operator +(RegexPattern pattern1, RegexPattern pattern2) =>
			new RegexPattern(pattern1._regexValue + pattern2._regexValue);
	}
}