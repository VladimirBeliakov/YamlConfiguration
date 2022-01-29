using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YamlConfiguration.Processor.Tests
{
	public abstract class QuotedMultiLineBaseTest
	{
		protected static IEnumerable<RegexTestCase> GetFirstLines(
			bool isDoubleQuoted,
			bool withClosingQuote = false
		)
		{
			var quote = isDoubleQuoted ? "\"" : "\'";
			var @break = BasicStructures.Break;

			var closingWhites = withClosingQuote ? CharStore.SpacesAndTabs : null;
			var closingChars = withClosingQuote ? closingWhites + quote : null;

			var nbNsInLineCases = getNbNsInLineCases(isFirstLine: true, isDoubleQuoted);

			foreach (var nbNsInLine in nbNsInLineCases)
				yield return new RegexTestCase(
					testValue: quote + nbNsInLine + closingChars + @break,
					wholeMatch: quote + nbNsInLine + closingChars,
					nbNsInLine,
					closingChars,
					closingWhites
				);

			yield return new RegexTestCase(
				testValue: quote + String.Empty + closingChars + @break,
				wholeMatch: quote + String.Empty + closingChars,
				String.Empty,
				closingChars,
				closingWhites
			);
		}

		protected static IEnumerable<RegexTestCase> GetNextLines(
			bool isDoubleQuoted,
			bool withClosingQuote = false
		)
		{
			var quote = isDoubleQuoted ? "\"" : "\'";
			var @break = Environment.NewLine;

			var closingWhites = withClosingQuote ? CharStore.SpacesAndTabs : null;
			var closingChars = withClosingQuote ? closingWhites + quote : null;

			var nbNsInLineCases = getNbNsInLineCases(isFirstLine: false, isDoubleQuoted);
			var anyNonSpaceCharGroup = nbNsInLineCases.First();

			yield return new RegexTestCase(
				testValue: anyNonSpaceCharGroup + closingChars + @break,
				wholeMatch: anyNonSpaceCharGroup + closingChars,
				anyNonSpaceCharGroup,
				closingChars,
				closingWhites
			);

			foreach (var nbNsInLine in nbNsInLineCases)
				yield return new RegexTestCase(
					testValue: nbNsInLine + closingChars + @break,
					wholeMatch: nbNsInLine + closingChars,
					nbNsInLine,
					closingChars,
					closingWhites
				);
		}

		private static IReadOnlyCollection<string> getNbNsInLineCases(bool isFirstLine, bool isDoubleQuoted)
		{
			return isDoubleQuoted
				? getNbNsFirstQuotedInLineFor(CharStore.NbNsDoubleCharsWithoutEscapedAndSurrogates.Value, isFirstLine)
						.Concat(getNbNsFirstQuotedInLineFor(CharStore.EscapedChars, isFirstLine))
						.Concat(getNbNsFirstQuotedInLineFor(CharStore.SurrogatePairs.Value, isFirstLine))
						.ToList()
				: getNbNsFirstQuotedInLineFor(CharStore.NbNsSingleCharsWithoutSurrogates.Value, isFirstLine)
					.Concat(getNbNsFirstQuotedInLineFor(CharStore.SurrogatePairs.Value, isFirstLine))
					.ToList();
		}

		private static IEnumerable<string> getNbNsFirstQuotedInLineFor(
			IReadOnlyCollection<string> nonSpaceChars,
			bool isFirstLine
		)
		{
			const int groupItemCount = Characters.CharGroupMaxLength;
			const int whiteCharGroupCount = groupItemCount / 2;
			const string tab = "\t";
			const string space = " ";

			var anyNonSpaceChar = nonSpaceChars.First();
			var nonSpaceCharLength = anyNonSpaceChar.Length;
			var nonSpaceCharGroupLength = nonSpaceCharLength * groupItemCount / 2;
			var oneGroupLength = nonSpaceCharLength + whiteCharGroupCount + nonSpaceCharGroupLength;

			var sb = new StringBuilder(oneGroupLength);
			sb.Append(anyNonSpaceChar);

			var isEvenIteration = false;
			foreach (var item in nonSpaceChars)
			{
				if (item.Length != nonSpaceCharLength)
					throw new InvalidOperationException(
						$"All value lengths of {nameof(nonSpaceChars)} must be equal to each other."
					);

				var whiteChar = isEvenIteration ? tab : space;

				sb.Append(whiteChar);
				sb.Append(item);

				isEvenIteration = !isEvenIteration;

				if (oneGroupLength == sb.Length)
				{
					yield return sb.ToString();
					sb.Clear();
					sb.Append(anyNonSpaceChar);
				}
			}

			if (sb.Length > 0)
				yield return sb.ToString();

			var prependedChar = isFirstLine ? String.Empty : anyNonSpaceChar;
			yield return prependedChar + String.Join(
				String.Empty,
				Enumerable.Repeat(CharStore.SpacesAndTabs + anyNonSpaceChar, groupItemCount)
			);

			yield return prependedChar + String.Join(String.Empty, nonSpaceChars.Take(groupItemCount));
		}

		protected static IEnumerable<string> GetFirstLineNegativeTestCases(bool isDoubleQuote)
		{
			var @break = Environment.NewLine;
			var tooManyWhiteSpaces = CharStore.GetCharRange(" \t") + " ";
			var tooManyNonSpaceChars = CharStore.GetCharRange("a ") + CharStore.GetCharRange("a ") + "a";
			var tooLongSeparateInLine = tooManyWhiteSpaces;
			var quote = isDoubleQuote ? "\"" : "\'";

			// Without a quote in the beginning
			yield return $" \\0A \t{@break}";

			if (isDoubleQuote)
			{
				// With \ and " inside nb ns double in line
				yield return $"{quote} \\1\\0A \t{@break}";
				yield return $"{quote} \" \\0A \t{@break}";
			}
			else
			{
				// With ' inside nb ns single in line
				yield return $"{quote} \' \\0A \t{@break}";
				yield return $"{quote} \' \\0A \t'{@break}";

				// Too many chars before the closing quote
				yield return $"{quote} \\0A \t{tooManyWhiteSpaces}\'{@break}";
			}

			// Too many chars
			yield return $"{quote}{tooManyWhiteSpaces}\\0A \t{@break}";
			yield return $"{quote}{tooManyNonSpaceChars} \t{@break}";
			yield return $"{quote} \\0A{tooLongSeparateInLine}{@break}";

			// Without a break (or no closing quote after the closing whites)
			yield return $"{quote}\t\\0A \t";
		}

		protected static IEnumerable<string> GetNextLineNegativeTestCases(bool isDoubleQuote)
		{
			var whites = CharStore.SpacesAndTabs;
			const string space = " ";

			var tooLongClosingWhites = whites + space;
			var tooManyNonSpaceChars = "a" + CharStore.GetCharRange("a ") + CharStore.GetCharRange("a ") + "a";
			var tooManyNonSpaceCharGroups = "a" + String.Join(
				String.Empty,
				Enumerable.Repeat(CharStore.SpacesAndTabs + "a", Characters.CharGroupMaxLength)
			) + "a";

			var lineEnding = isDoubleQuote ? "\"" : "\'";

			if (isDoubleQuote)
			{
				// With \ and " inside nb ns double in line
				yield return $" \\1\\0A \t{lineEnding}";
				yield return $" \" \\0A \t{lineEnding}";
			}
			else
			{
				// With ' inside nb ns single in line
				yield return $" \' \\0A \t{lineEnding}";
			}

			// Too many chars
			yield return $" {tooManyNonSpaceChars} \t{lineEnding}";
			yield return $" {tooManyNonSpaceCharGroups} \t{lineEnding}";
			yield return $" \\0A{tooLongClosingWhites}{lineEnding}";

			// Invalid non empty last line
			yield return " \\0A \t";
		}
	}
}
