using System;
using System.Linq;

namespace YamlConfiguration.Processor
{
	public readonly struct RegexPattern : IEquatable<RegexPattern>
	{
		public static RegexPattern Empty = (RegexPattern) String.Empty;

		private readonly string _regexValue;

		public RegexPattern(string regexValue)
		{
			_regexValue = regexValue;
		}

		public override string ToString() => _regexValue;

		public static implicit operator string(RegexPattern pattern) => pattern._regexValue;

		public static implicit operator char(RegexPattern pattern)
		{
			if (pattern._regexValue.Length == 1)
				return pattern._regexValue.Single();

			throw new InvalidCastException($"Can't cast the multi char pattern '{pattern}' to one char.");
		}

		public static explicit operator RegexPattern(string rawValue) => new RegexPattern(rawValue);

		public static explicit operator RegexPattern(char rawValue) => new RegexPattern(rawValue.ToString());

		public static RegexPattern operator +(RegexPattern pattern1, RegexPattern pattern2) =>
			new RegexPattern($"{pattern1._regexValue}{pattern2._regexValue}");

		public bool Equals(RegexPattern other)
		{
			return _regexValue == other._regexValue;
		}

		public override bool Equals(object? obj)
		{
			return obj is RegexPattern other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _regexValue.GetHashCode();
		}

		public static bool operator ==(RegexPattern left, RegexPattern right)
		{
			return left._regexValue == right._regexValue;
		}

		public static bool operator !=(RegexPattern left, RegexPattern right)
		{
			return left._regexValue != right._regexValue;
		}
	}
}