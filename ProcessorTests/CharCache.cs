using System;
using System.Collections.Generic;
using System.Linq;
using Processor;
using ProcessorTests.Extensions;

namespace ProcessorTests
{
	internal static class CharCache
	{
		private static string getCharRange(string chars)
		{
			return String.Join(
				String.Empty,
				Enumerable.Repeat(chars, Characters.CharGroupLength / chars.Length)
			);
		}

		public static string Spaces = getCharRange(" ");

		public static string Tabs = getCharRange("\t");

		public static string SpacesAndTabs = getCharRange("\t ");

		public static string Chars = getCharRange("ABCD");

		public static string Digits = getCharRange("0123456789");

		public static IReadOnlyCollection<string> SeparateInLineCases = new[] { " ", "\t", Spaces, Tabs, SpacesAndTabs };

		public static IEnumerable<string> GetTagHandles()
		{
			yield return "!";
			yield return "!!";

			foreach (var wordChar in WordChars.GroupBy(Characters.CharGroupLength))
			{
				yield return $"!{wordChar}";
				yield return $"!{wordChar}!";
			}
		}

		public static IEnumerable<string> GetComments()
		{
			var @break = Environment.NewLine;

			return new[] { String.Empty + @break, $" #{@break}", $" #{Chars + Chars}{@break}" };
		}

		public static IEnumerable<string> GetUriCharGroups()
		{
			var hexNumberGroups = GetHexNumbers().GroupBy(Characters.CharGroupLength);
			var uriCharGroupsWithoutHexNumbers = GetUriCharsWithoutHexNumbers().GroupBy(Characters.CharGroupLength);

			return hexNumberGroups.Concat(uriCharGroupsWithoutHexNumbers);
		}

		public static IEnumerable<string> GetHexNumbers()
		{
			var hexDigits = _hexLetters.Concat(_decimalDigits).ToList();

			return from firstHexDigit in hexDigits
				   from secondHexDigit in hexDigits
				   select "%" + firstHexDigit + secondHexDigit;
		}

		public static IEnumerable<string> GetUriCharsWithoutHexNumbers()
		{
			var additionalUriChar = new[]
			{
				"#", ";", "/", "?", ":", "@", "&", "=", "+", "$", ",", "_", ".", "!", "~", "*", "'", "(", ")",
				"[", "]", "”"
			};

			return additionalUriChar.Concat(WordChars);
		}

		private static readonly IEnumerable<string> _decimalDigits =
			new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

		private static readonly IEnumerable<string> _hexLetters =
			new[] { "A", "B", "C", "D", "E", "F", "a", "b", "c", "d", "e", "f" };

		private static readonly IEnumerable<string> _asciiLetters = _hexLetters.Concat(
			new[]
			{
				"G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
				"g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
			}
		);

		public static readonly IEnumerable<string> WordChars = _decimalDigits.Concat(_asciiLetters);
	}
}