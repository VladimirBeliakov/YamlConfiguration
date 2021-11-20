using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace YamlConfiguration.Processor.Tests
{
	internal static class CharStore
	{
		private static readonly IEnumerable<string> _decimalDigits =
			new[]
			{
				"0",
#if RELEASE
				"1", "2", "3", "4", "5", "6", "7", "8", "9"
#endif
			};

		private static readonly IEnumerable<string> _hexLetters =
			new[]
			{
				"A",
#if RELEASE
				"B", "C", "D", "E", "F",
#endif
				"a",
#if RELEASE
				"b", "c", "d", "e", "f"
#endif
			};

		private static readonly IEnumerable<string> _asciiLetters = _hexLetters.Concat(
			new[]
			{
				"G",
#if RELEASE
				"H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
#endif
				"g",
#if RELEASE
				"h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
#endif
			}
		);

		public static string Spaces = GetCharRange(" ");

		public static string Tabs = GetCharRange("\t");

		public static string SpacesAndTabs = GetCharRange("\t ");

		public static string Chars = GetCharRange(String.Join(String.Empty, _asciiLetters));

		public static string Digits = GetCharRange(String.Join(String.Empty, _decimalDigits));

		public static IReadOnlyCollection<string> SeparateInLineCases =
			new[]
			{
				" ",
#if RELEASE
				"\t", Spaces, Tabs, SpacesAndTabs
#endif
			};

		public static readonly IEnumerable<string> FlowIndicators = new[]
		{
			",",
#if RELEASE
			"[", "]", "{", "}"
#endif
		};

		public static readonly IEnumerable<string> CIndicators = new[]
		{
			"-",
#if RELEASE
			"?", ":", "#", "&", "*", "!", "|", ">", "'", "\"", "%", "@", "`"
#endif
		}.Concat(FlowIndicators);

		public static readonly IEnumerable<string> WordChars = _decimalDigits.Concat(_asciiLetters);

		public static readonly IReadOnlyCollection<string> EscapedChars = new[]
		{
			"\\0",
#if RELEASE
			"\\a", "\\b", "\\t", "\\n", "\\v", "\\f", "\\r", "\\e", "\\ ", "\\\"",
			"\\/", "\\\\", "\\N", "\\\u00A0", "\\L", "\\P", "\\x", "\\u", "\\U",
#endif
		};

		public static string GetCharRange(string chars)
		{
			var result = String.Join(
				String.Empty,
				Enumerable.Repeat(chars, Characters.CharGroupMaxLength / chars.Length)
			);

			var missingCharsCount = Characters.CharGroupMaxLength - result.Length;

			if (missingCharsCount > 0)
				result += new String(result.Take(missingCharsCount).ToArray());

			return result;
		}

		public static string Repeat(char @char, int times) =>
			String.Join(String.Empty, Enumerable.Repeat(@char, times));

		public static IEnumerable<string> GetTagHandles()
		{
			yield return "!";
			yield return "!!";

			foreach (var wordChar in WordChars.GroupBy(Characters.CharGroupMaxLength))
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
			var hexNumberGroups = GetHexNumbers().GroupBy(Characters.CharGroupMaxLength);
			var uriCharGroupsWithoutHexNumbers = GetUriCharsWithoutHexNumbers().GroupBy(Characters.CharGroupMaxLength);

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
				"#",
#if RELEASE
				";", "/", "?", ":", "@", "&", "=", "+", "$", ",", "_", ".", "!", "~", "*", "'", "(", ")", "[", "]", "”"
#endif
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
				.GroupBy(Characters.CharGroupMaxLength);

			var surrogatePairGroups = SurrogatePairs.Value.Except(excludedChars).GroupBy(Characters.CharGroupMaxLength);

			return oneCharGroups.Concat(surrogatePairGroups);
		}

		private static readonly Lazy<IReadOnlyCollection<string>> _printableCharsWithoutSurrogates =
			new(() =>
				{
					const char tag = '\u0009';
					const char lf = '\u000A';
					const char cr = '\u000D';
					const char nel = '\u0085';

#if RELEASE
					var basicLatinSubset = getCharSequence(0x20, 0x7E);
					var latinSupplementToHangulJamo = getCharSequence(0x00A0, 0xD7FF);
					var privateUseAreaToSpecialsBeginning = getCharSequence(0xE000, 0xFFFD);
#endif

					return
#if RELEASE
						basicLatinSubset
#else
						new char[] {}
						.Append((char) 0x20)
						.Append((char) 0x7E)
#endif
#if RELEASE
						.Concat(latinSupplementToHangulJamo)
#else
						.Append((char) 0x00A0)
						.Append((char) 0xD7FF)
#endif
#if RELEASE
						.Concat(privateUseAreaToSpecialsBeginning)
#else
						.Append((char) 0xE000)
						.Append((char) 0xFFFD)
#endif
						.Append(tag)
						.Append(lf)
						.Append(cr)
						.Append(nel)
						.Select(c => c.ToString()).ToList();
				},
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		private static readonly Lazy<IReadOnlyCollection<string>> _nbDoubleCharsWithoutEscapedAndSurrogates =
			new(
				() => getJsonCharsWithoutSurrogates(new[] { '\\', '\"' }).ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static readonly Lazy<IReadOnlyCollection<string>> NbNsDoubleCharsWithoutEscapedAndSurrogates =
			new(
				() =>
					_nbDoubleCharsWithoutEscapedAndSurrogates.Value.Except(new[] { " ", "\t" }).ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		private static readonly Lazy<IReadOnlyCollection<string>> _nbSingleCharsWithoutSurrogates =
			new(
				() => getJsonCharsWithoutSurrogates(new[] { '\'' }).ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static readonly Lazy<IReadOnlyCollection<string>> NbNsSingleCharsWithoutSurrogates =
			new(
				() => _nbSingleCharsWithoutSurrogates.Value.Except(new[] { " ", "\t" }).ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static readonly Lazy<IReadOnlyCollection<string>> SurrogatePairs =
			new(
				() =>
#if RELEASE
					(from highSurrogate in getCharSequence(start: 0xD800, end: 0xDBFF)
					 from lowSurrogate in getCharSequence(start: 0xDC00, end: 0xDFFF)
					 select new String(new[] { highSurrogate, lowSurrogate }))
					.ToList()
#else
				   new [] {
					   new String(new[] { (char) 0xD800, (char) 0xDC00 }),
					   new String(new[] { (char) 0xDBFF, (char) 0xDFFF }),
				   }
#endif
			   ,
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		private static IEnumerable<string> getJsonCharsWithoutSurrogates(IEnumerable<char> andTheseChars)
		{
			return
#if RELEASE
				getCharSequence(start: 0x0020, end: 0xD7FF)
#else
				Array.Empty<char>()
				.Append((char) 0x0020)
				.Append((char) 0xD7FF)
#endif
#if RELEASE
				.Concat(getCharSequence(start: 0xE000, end: 0xFFFF))
#else
				.Append((char) 0xE000)
				.Append((char) 0xFFFF)
#endif
				.Append('\t')
				.Except(andTheseChars)
				.Select(c => c.ToString());
		}

#if RELEASE
		public static IEnumerable<char> getCharSequence(int start, int end)
		{
			return Enumerable.Range(start, end - start + 1).Select(c => (char) c);
		}
#endif
	}
}
