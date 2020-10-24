using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Processor;
using Processor.FlowStyles;

namespace ProcessorTests
{
	public abstract class QuotedMultiLineBaseTest
	{
		protected static IEnumerable<MultiLineOneLineTestCase> GetFirstLines(
			bool isDoubleQuoted,
			bool withEscapedBreak = false
		)
		{
			var quote = isDoubleQuoted ? "\"" : "\'";
			var @break = withEscapedBreak && isDoubleQuoted ? "\\" + Environment.NewLine : Environment.NewLine;
			var chars = CharStore.Chars;

			var separateInLineCases = CharStore.SeparateInLineCases;

			var nbNsInLineCases = getNbNsInLineCases(isFirstLine: true, isDoubleQuoted);
			var anySeparateInLine = separateInLineCases.First();
			var anyNonSpaceCharGroup = nbNsInLineCases.First();

			string trailingWhiteSpaces;
			foreach (var nbNsInLine in nbNsInLineCases)
			{
				trailingWhiteSpaces = withEscapedBreak && isDoubleQuoted ? anySeparateInLine : string.Empty;

				yield return new MultiLineOneLineTestCase(
					testValue: quote + nbNsInLine + anySeparateInLine + @break + chars,
					result: ProcessedLineResult.First(nbNsInLine + trailingWhiteSpaces)
				);
			}

			foreach (var separateInLine in separateInLineCases.Append(string.Empty))
			{
				trailingWhiteSpaces = withEscapedBreak && isDoubleQuoted ? separateInLine : string.Empty;

				yield return new MultiLineOneLineTestCase(
					testValue: quote + anyNonSpaceCharGroup + separateInLine + @break + chars,
					result: ProcessedLineResult.First(anyNonSpaceCharGroup + trailingWhiteSpaces)
				);
			}

			trailingWhiteSpaces = withEscapedBreak && isDoubleQuoted ? anySeparateInLine : string.Empty;

			yield return new MultiLineOneLineTestCase(
				testValue: quote + string.Empty + anySeparateInLine + @break + chars,
				result: ProcessedLineResult.First(string.Empty + trailingWhiteSpaces)
			);
		}

		protected static IEnumerable<MultiLineOneLineTestCase> GetEmptyNextLines(
			bool isDoubleQuoted,
			bool isLastLine = false
		)
		{
			var @break = Environment.NewLine;
			var quote = isDoubleQuoted ? "\"" : "\'";
			var lineEnding = isLastLine ? quote : @break;
			var indent = CharStore.Spaces;
			var separateInLineCases = CharStore.SeparateInLineCases;

			foreach (var separateInLine in separateInLineCases.Append(string.Empty))
			{
				yield return new MultiLineOneLineTestCase(
					testValue: indent + separateInLine + lineEnding,
					result: isLastLine ? ProcessedLineResult.LastEmpty() : ProcessedLineResult.Empty()
				);

				yield return new MultiLineOneLineTestCase(
					testValue: separateInLine + lineEnding,
					result: isLastLine ? ProcessedLineResult.LastEmpty() : ProcessedLineResult.Empty()
				);
			}
		}

		protected static IEnumerable<MultiLineOneLineTestCase> GetNonEmptyLines(
			bool isDoubleQuoted,
			bool withEscapedBreak = false,
			bool isLastLine = false
		)
		{
			if (!isDoubleQuoted && withEscapedBreak)
				throw new InvalidOperationException("Only double quoted lines have escaped breaks.");

			if (withEscapedBreak && isLastLine)
				throw new InvalidOperationException("Last line can't be with an escaped break.");

			var quote = isDoubleQuoted ? "\"" : "\'";
			var @break = withEscapedBreak && isDoubleQuoted ? "\\" + Environment.NewLine : Environment.NewLine;
			var lineEnding = isLastLine ? quote : @break;
			var indent = CharStore.Spaces;
			var separateInLineCases = CharStore.SeparateInLineCases;

			var nbNsInLineCases = getNbNsInLineCases(isFirstLine: false, isDoubleQuoted);
			var anyNonSpaceCharGroup = nbNsInLineCases.First();
			var anySeparateInLine = separateInLineCases.First();
			var anyLinePrefix = anySeparateInLine;

			foreach (var separateInLine in separateInLineCases.Append(string.Empty))
			{
				var trailingWhiteSpaces =
					withEscapedBreak && isDoubleQuoted || isLastLine ? separateInLine : string.Empty;

				var testValue = anyNonSpaceCharGroup + trailingWhiteSpaces;
				var linePrefix = indent + separateInLine;

				yield return new MultiLineOneLineTestCase(
					testValue: linePrefix + anyNonSpaceCharGroup + separateInLine + lineEnding,
					result: isLastLine
						? ProcessedLineResult.LastNotEmpty(testValue)
						: ProcessedLineResult.NotEmpty(testValue)
				);
			}

			foreach (var nbNsInLine in nbNsInLineCases)
			{
				var trailingWhiteSpaces =
					withEscapedBreak && isDoubleQuoted || isLastLine ? anySeparateInLine : string.Empty;

				var testValue = nbNsInLine + trailingWhiteSpaces;

				yield return new MultiLineOneLineTestCase(
					testValue: anyLinePrefix + nbNsInLine + anySeparateInLine +
							   lineEnding,
					result: isLastLine
						? ProcessedLineResult.LastNotEmpty(testValue)
						: ProcessedLineResult.NotEmpty(testValue)
				);
			}
		}

		private static IReadOnlyCollection<string> getNbNsInLineCases(bool isFirstLine, bool isDoubleQuoted)
		{
			if (isDoubleQuoted)
			{
				return
					getNbNsFirstQuotedInLineFor(CharStore.NbNsDoubleCharsWithoutEscapedAndSurrogates.Value, isFirstLine)
						.Concat(getNbNsFirstQuotedInLineFor(CharStore.EscapedChars, isFirstLine))
						.Concat(getNbNsFirstQuotedInLineFor(CharStore.SurrogatePairs.Value, isFirstLine))
						.ToList();
			}

			return
				getNbNsFirstQuotedInLineFor(CharStore.NbNsSingleCharsWithoutSurrogates.Value, isFirstLine)
					.Concat(getNbNsFirstQuotedInLineFor(CharStore.SurrogatePairs.Value, isFirstLine))
					.ToList();
		}

		private static IEnumerable<string> getNbNsFirstQuotedInLineFor(
			IReadOnlyCollection<string> nonSpaceChars,
			bool isFirstLine
		)
		{
			const int groupItemCount = Characters.CharGroupLength;
			const int whiteCharGroupCount = groupItemCount / 2;

			var nonSpaceChar = nonSpaceChars.First();
			var nonSpaceCharLength = nonSpaceChar.Length;
			var nonSpaceCharGroupCount = nonSpaceCharLength * groupItemCount / 2;
			var oneGroupLength = nonSpaceCharLength + whiteCharGroupCount + nonSpaceCharGroupCount;

			var sb = new StringBuilder(oneGroupLength);
			sb.Append(nonSpaceChar);

			var isEvenIteration = false;
			foreach (var item in nonSpaceChars)
			{
				if (item.Length != nonSpaceCharLength)
					throw new InvalidOperationException(
						$"All value lengths of {nameof(nonSpaceChars)} must be equal to each other."
					);

				var whiteChar = isEvenIteration ? "\t" : " ";

				sb.Append(whiteChar);
				sb.Append(item);

				isEvenIteration = !isEvenIteration;

				if (oneGroupLength == sb.Length)
				{
					yield return sb.ToString();
					sb.Clear();
					sb.Append(nonSpaceChar);
				}
			}

			if (sb.Length > 0)
				yield return sb.ToString();

			var prependedChar = isFirstLine ? string.Empty : nonSpaceChar;
			yield return prependedChar + string.Join(
				string.Empty,
				Enumerable.Repeat(CharStore.SpacesAndTabs + nonSpaceChar, groupItemCount)
			);

			yield return string.Join(string.Empty, nonSpaceChars.Take(groupItemCount));
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
			}

			// Too many chars
			yield return $"{quote}{tooManyWhiteSpaces}\\0A \t{@break}";
			yield return $"{quote}{tooManyNonSpaceChars} \t{@break}";
			yield return $"{quote} \\0A{tooLongSeparateInLine}{@break}";

			// Without a break
			yield return $"{quote}\t\\0A \t";
		}

		protected static IEnumerable<string> GetNextLineNegativeTestCases(bool isDoubleQuote)
		{
			var @break = Environment.NewLine;
			var indent = CharStore.Spaces;
			var separateInLine = CharStore.GetCharRange(" \t");
			const string space = " ";

			var tooLongIndent = indent + space;
			var tooLongSeparateInLine = separateInLine + space;
			var tooManyNonSpaceChars = "a" + CharStore.GetCharRange("a ") + CharStore.GetCharRange("a ") + "a";
			var tooManyNonSpaceCharGroups = "a" + string.Join(
				string.Empty,
				Enumerable.Repeat(CharStore.SpacesAndTabs + "a", Characters.CharGroupLength)
			) + "a";

			foreach (var lineEnding in new[] { @break, isDoubleQuote ? "\"" : "\'" })
			{
				// Invalid empty lines
				yield return $"{indent}{tooLongSeparateInLine}{lineEnding}";
				yield return $"{tooLongIndent}{separateInLine}{lineEnding}";

				if (isDoubleQuote)
				{
					// Invalid non empty lines
					// With \ and " inside nb ns double in line
					yield return $" \\1\\0A \t{lineEnding}";
					yield return $" \" \\0A \t{lineEnding}";
				}
				else
				{
					// Invalid non empty lines
					// With ' inside nb ns single in line
					yield return $" \' \\0A \t{lineEnding}";
				}

				// Too many chars
				yield return $" {tooManyNonSpaceChars} \t{lineEnding}";
				yield return $" {tooManyNonSpaceCharGroups} \t{lineEnding}";
				yield return $" \\0A{tooLongSeparateInLine}{lineEnding}";
			}

			// Invalid empty last line
			yield return $"{string.Empty}";

			// Invalid non empty last line
			yield return " \\0A \t";
		}
	}
}