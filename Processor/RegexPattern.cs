using System;

namespace Processor
{
	public class RegexPattern : IEquatable<RegexPattern>
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

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((RegexPattern) obj);
		}

		public bool Equals(RegexPattern? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _regexValue == other._regexValue;
		}

		public override int GetHashCode()
		{
			return _regexValue.GetHashCode();
		}

		public static bool operator ==(RegexPattern? left, RegexPattern? right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(RegexPattern? left, RegexPattern? right)
		{
			return !Equals(left, right);
		}
	}
}