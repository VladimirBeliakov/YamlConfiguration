using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace YamlConfiguration.Processor.Tests
{
	internal static class CharStore
	{
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

		public static string Spaces = GetCharRange(" ");

		public static string Tabs = GetCharRange("\t");

		public static string SpacesAndTabs = GetCharRange("\t ");

		public static string Chars = GetCharRange("ABCD");

		public static string Digits = GetCharRange("0123456789");

		public static IReadOnlyCollection<string> SeparateInLineCases =
			new[] { " ", "\t", Spaces, Tabs, SpacesAndTabs };

		public static readonly IEnumerable<string> FlowIndicators = new[] { ",", "[", "]", "{", "}" };

		public static readonly IEnumerable<string> CIndicators = new[]
		{
			"-", "?", ":", "#", "&", "*", "!", "|", ">", "'", "\"", "%", "@", "`"
		}.Concat(FlowIndicators);

		public static readonly IEnumerable<string> WordChars = _decimalDigits.Concat(_asciiLetters);

		public static readonly IReadOnlyCollection<string> EscapedChars = new[]
		{
			"\\0", "\\a", "\\b", "\\t", "\\n", "\\v", "\\f", "\\r", "\\e", "\\ ", "\\\"",
			"\\/", "\\\\", "\\N", "\\\u00A0", "\\L", "\\P", "\\x", "\\u", "\\U",
		};

		public static string GetCharRange(string chars)
		{
			return String.Join(
				String.Empty,
				Enumerable.Repeat(chars, Characters.CharGroupLength / chars.Length)
			);
		}

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

		public static IEnumerable<string> GetTagChars()
		{
			var notAllowedChars = FlowIndicators.Append("!");
			var uriChars = GetUriCharsWithoutHexNumbers().Concat(GetHexNumbers());

			return uriChars.Except(notAllowedChars);
		}

		public static IEnumerable<string> GetNsCharGroups(IEnumerable<string>? excludedChars = null)
		{
			const string lf = "\u000A";
			const string cr = "\u000D";
			const string byteOrderMark = "\uFEFF";
			const string space = " ";
			const string tab = "\t";

			var allExcludedChars =
				(excludedChars ?? Enumerable.Empty<string>()).Concat(new[] { lf, cr, byteOrderMark, space, tab });

			return GetPrintableCharGroups(excludedChars: allExcludedChars.ToList());
		}

		public static IEnumerable<string> GetNsCharsWithoutSurrogates(IEnumerable<string>? excludedChars = null)
		{
			const string lf = "\u000A";
			const string cr = "\u000D";
			const string byteOrderMark = "\uFEFF";
			const string space = " ";
			const string tab = "\t";

			var allExcludedChars =
				(excludedChars ?? Enumerable.Empty<string>()).Concat(new[] { lf, cr, byteOrderMark, space, tab });

			return _printableCharsWithoutSurrogates.Value.Except(allExcludedChars);
		}

		public static IEnumerable<string> GetPrintableCharGroups(IReadOnlyCollection<string> excludedChars)
		{
			var oneCharGroups = _printableCharsWithoutSurrogates.Value
				.Except(excludedChars)
				.GroupBy(Characters.CharGroupLength);

			var surrogatePairGroups = SurrogatePairs.Value.Except(excludedChars).GroupBy(Characters.CharGroupLength);

			return oneCharGroups.Concat(surrogatePairGroups);
		}

		private static readonly Lazy<IReadOnlyCollection<string>> _printableCharsWithoutSurrogates =
			new Lazy<IReadOnlyCollection<string>>(
				() =>
				{
					const char tag = '\u0009';
					const char lf = '\u000A';
					const char cr = '\u000D';
					const char nel = '\u0085';

					var basicLatinSubset = getCharSequence(0x20, 0x7E);
					var latinSupplementToHangulJamo = getCharSequence(0x00A0, 0xD7FF);
					var privateUseAreaToSpecialsBeginning = getCharSequence(0xE000, 0xFFFD);

					return basicLatinSubset
						.Concat(latinSupplementToHangulJamo)
						.Concat(privateUseAreaToSpecialsBeginning)
						.Append(tag)
						.Append(lf)
						.Append(cr)
						.Append(nel)
						.Select(c => c.ToString()).ToList();
				},
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		private static readonly Lazy<IReadOnlyCollection<string>> _nbDoubleCharsWithoutEscapedAndSurrogates =
			new Lazy<IReadOnlyCollection<string>>(
				() => getJsonCharsWithoutSurrogates(new[] { '\\', '\"' }).ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static readonly Lazy<IReadOnlyCollection<string>> NbNsDoubleCharsWithoutEscapedAndSurrogates =
			new Lazy<IReadOnlyCollection<string>>(
				() =>
					_nbDoubleCharsWithoutEscapedAndSurrogates.Value.Except(new[] { " ", "\t" }).ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static readonly Lazy<IReadOnlyCollection<string>> _nbSingleCharsWithoutSurrogates =
			new Lazy<IReadOnlyCollection<string>>(
				() => getJsonCharsWithoutSurrogates(new[] { '\'' }).ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static readonly Lazy<IReadOnlyCollection<string>> NbNsSingleCharsWithoutSurrogates =
			new Lazy<IReadOnlyCollection<string>>(
				() => _nbSingleCharsWithoutSurrogates.Value.Except(new[] { " ", "\t" }).ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static readonly Lazy<IReadOnlyCollection<string>> SurrogatePairs =
			new Lazy<IReadOnlyCollection<string>>(
				() =>
					(from highSurrogate in getCharSequence(start: 0xD800, end: 0xDBFF)
					 from lowSurrogate in getCharSequence(start: 0xDC00, end: 0xDFFF)
					 select new String(new[] { highSurrogate, lowSurrogate }))
					.ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		private static IEnumerable<string> getJsonCharsWithoutSurrogates(IEnumerable<char> andTheseChars)
		{
			return getCharSequence(start: 0x0020, end: 0xD7FF)
				.Concat(getCharSequence(start: 0xE000, end: 0xFFFF))
				.Append('\t')
				.Except(andTheseChars)
				.Select(c => c.ToString());
		}

		public static IEnumerable<char> getCharSequence(int start, int end)
		{
			return Enumerable.Range(start, end - start + 1).Select(c => (char) c);
		}
	}
}
