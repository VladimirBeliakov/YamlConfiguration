using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Processor;
using Processor.FlowStyles;

namespace ProcessorTests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class MultiLineTests
	{
		[TestCaseSource(nameof(getFirstLinePositiveTestCases))]
		public void ValidFirstLine_ReturnsCorrectLineTypeAndExtractedValue(OneLineTestCase testCase)
		{
			var multiLine = new DoubleQuotedStyles.MultiLine();

			var firstLineResult = multiLine.ProcessFirstLine(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(firstLineResult.LineType, Is.EqualTo(testCase.Result.LineType));
					Assert.That(firstLineResult.ExtractedValue, Is.EqualTo(testCase.Result.ExtractedValue));
				}
			);
		}

		[TestCaseSource(nameof(getNextLinePositiveTestCases))]
		public void ValidNextLine_ReturnsCorrectLineTypeAndExtractedValue(OneLineTestCase testCase)
		{
			var multiLine = new DoubleQuotedStyles.MultiLine();
			multiLine.ProcessFirstLine("\"");

			var nextLineResult = multiLine.ProcessNextLine(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(nextLineResult.LineType, Is.EqualTo(testCase.Result.LineType));
					Assert.That(nextLineResult.ExtractedValue, Is.EqualTo(testCase.Result.ExtractedValue));
				}
			);
		}

		[TestCaseSource(nameof(getFirstLineNegativeTestCases))]
		public void InvalidFirstLine_ReturnsInvalid(string testCase)
		{
			var multiLine = new DoubleQuotedStyles.MultiLine();

			var firstLineResult = multiLine.ProcessFirstLine(testCase);

			Assert.That(firstLineResult.LineType, Is.EqualTo(LineType.Invalid), getExceptionMessage: () => testCase);
		}

		[TestCaseSource(nameof(getNextLineNegativeTestCases))]
		public void InvalidNextLine_ReturnsInvalid(string testCase)
		{
			var multiLine = new DoubleQuotedStyles.MultiLine();
			multiLine.ProcessFirstLine("\"");

			var nextLineResult = multiLine.ProcessNextLine(testCase);

			Assert.That(nextLineResult.LineType, Is.EqualTo(LineType.Invalid), getExceptionMessage: () => testCase);
		}

		[Test]
		public void ProcessFirstLine_CalledTwice_Throws()
		{
			var multiLine = new DoubleQuotedStyles.MultiLine();
			multiLine.ProcessFirstLine("\"");

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessFirstLine("\""));
		}

		[Test]
		public void ProcessNextLine_FirstLineWasNotProcessed_Throws()
		{
			var multiLine = new DoubleQuotedStyles.MultiLine();

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessNextLine(string.Empty));
		}

		[Test]
		public void ProcessNextLine_LastLineAlreadyProcessed_Throws()
		{
			var multiLine = new DoubleQuotedStyles.MultiLine();
			multiLine.ProcessFirstLine("\"");
			multiLine.ProcessNextLine("\"");

			Assert.Throws<InvalidOperationException>(() => multiLine.ProcessNextLine(string.Empty));
		}

		private static readonly Lazy<IReadOnlyCollection<string>> _nbNsDoubleInLineCases =
			new Lazy<IReadOnlyCollection<string>>(
				() => getNbNsDoubleInLineFor(CharStore.NonSpaceDoubleCharsWithoutEscapedAndSurrogates)
					.Concat(getNbNsDoubleInLineFor(CharStore.EscapedChars))
					.Concat(getNbNsDoubleInLineFor(CharStore.SurrogatePairs))
					.ToList(),
				LazyThreadSafetyMode.ExecutionAndPublication
			);

		private static IEnumerable<OneLineTestCase> getFirstLinePositiveTestCases() =>
			getFirstLines(withEscapedBreak: false)
				.Concat(getFirstLines(withEscapedBreak: true));

		private static IEnumerable<OneLineTestCase> getNextLinePositiveTestCases() =>
			getEmptyNextLines()
				.Concat(getNonEmptyLines(withEscapedBreak: false))
				.Concat(getNonEmptyLines(withEscapedBreak: true))
				.Concat(getEmptyNextLines(isLastLine: true))
				.Concat(getNonEmptyLines(isLastLine: true));

		private static IEnumerable<OneLineTestCase> getFirstLines(bool withEscapedBreak)
		{
			var @break = withEscapedBreak ? "\\" + Environment.NewLine : Environment.NewLine;
			var chars = CharStore.Chars;

			var separateInLineCases = CharStore.SeparateInLineCases;

			var anySeparateInLine = separateInLineCases.First();
			var anyNonSpaceDoubleCharGroup = _nbNsDoubleInLineCases.Value.First();

			string trailingWhiteSpaces;
			foreach (var nbNsDoubleInLine in _nbNsDoubleInLineCases.Value)
			{
				trailingWhiteSpaces = withEscapedBreak ? anySeparateInLine : string.Empty;

				yield return new OneLineTestCase(
					testValue: "\"" + nbNsDoubleInLine + anySeparateInLine + @break + chars,
					result: ProcessedLineResult.First(nbNsDoubleInLine + trailingWhiteSpaces)
				);
			}

			foreach (var separateInLine in separateInLineCases.Append(string.Empty))
			{
				trailingWhiteSpaces = withEscapedBreak ? separateInLine : string.Empty;

				yield return new OneLineTestCase(
					testValue: "\"" + anyNonSpaceDoubleCharGroup + separateInLine + @break + chars,
					result: ProcessedLineResult.First(anyNonSpaceDoubleCharGroup + trailingWhiteSpaces)
				);
			}

			trailingWhiteSpaces = withEscapedBreak ? anySeparateInLine : string.Empty;

			yield return new OneLineTestCase(
				testValue: "\"" + string.Empty + anySeparateInLine + @break + chars,
				result: ProcessedLineResult.First(string.Empty + trailingWhiteSpaces)
			);
		}

		private static IEnumerable<string> getNbNsDoubleInLineFor(IReadOnlyCollection<string> nonSpaceDoubleChars)
		{
			const int groupItemCount = Characters.CharGroupLength;
			const int groupWhiteCharCount = groupItemCount / 2;

			var nonSpaceDoubleCharLength = nonSpaceDoubleChars.First().Length;
			var groupNonSpaceDoubleCharCount = nonSpaceDoubleCharLength * groupItemCount / 2;
			var oneGroupLength = groupWhiteCharCount + groupNonSpaceDoubleCharCount;

			var sb = new StringBuilder(oneGroupLength);

			var isEvenIteration = false;
			foreach (var item in nonSpaceDoubleChars)
			{
				if (item.Length != nonSpaceDoubleCharLength)
					throw new InvalidOperationException(
						$"All value lengths of {nonSpaceDoubleChars} must be equal to each other."
					);

				var whiteChar = isEvenIteration ? "\t" : " ";

				sb.Append(whiteChar);
				sb.Append(item);

				isEvenIteration = !isEvenIteration;

				if (oneGroupLength == sb.Length)
				{
					yield return sb.ToString();
					sb.Clear();
				}
			}

			if (sb.Length > 0)
				yield return sb.ToString();

			yield return string.Join(string.Empty, nonSpaceDoubleChars.Take(groupItemCount));
		}

		private static IEnumerable<OneLineTestCase> getEmptyNextLines(bool isLastLine = false)
		{
			var @break = Environment.NewLine;
			var lineEnding = isLastLine ? "\"" : @break;
			var indent = CharStore.Spaces;
			var separateInLineCases = CharStore.SeparateInLineCases;

			foreach (var separateInLine in separateInLineCases.Append(string.Empty))
			{
				yield return new OneLineTestCase(
					testValue: indent + separateInLine + lineEnding,
					result: isLastLine ? ProcessedLineResult.LastEmpty() : ProcessedLineResult.Empty()
				);

				yield return new OneLineTestCase(
					testValue: separateInLine + lineEnding,
					result: isLastLine ? ProcessedLineResult.LastEmpty() : ProcessedLineResult.Empty()
				);
			}
		}

		private static IEnumerable<OneLineTestCase> getNonEmptyLines(
			bool withEscapedBreak = false,
			bool isLastLine = false
		)
		{
			if (withEscapedBreak && isLastLine)
				throw new InvalidOperationException("Last line can't be with an escaped break");

			var @break = withEscapedBreak ? "\\" + Environment.NewLine : Environment.NewLine;
			var lineEnding = isLastLine ? "\"" : @break;
			var indent = CharStore.Spaces;
			var separateInLineCases = CharStore.SeparateInLineCases;

			var anyNonSpaceDoubleCharGroup = _nbNsDoubleInLineCases.Value.First();
			var anyNonSpaceDoubleChar = anyNonSpaceDoubleCharGroup[1].ToString();
			var anySeparateInLine = separateInLineCases.First();
			var anyLinePrefix = anySeparateInLine;

			foreach (var separateInLine in separateInLineCases.Append(string.Empty))
			{
				var trailingWhiteSpaces = withEscapedBreak || isLastLine ? separateInLine : string.Empty;

				var linePrefix = indent + separateInLine;

				yield return new OneLineTestCase(
					testValue: linePrefix + anyNonSpaceDoubleChar + anyNonSpaceDoubleCharGroup + separateInLine +
							   lineEnding,
					result: isLastLine
						? ProcessedLineResult.LastNotEmpty(
							anyNonSpaceDoubleChar + anyNonSpaceDoubleCharGroup + trailingWhiteSpaces
						)
						: ProcessedLineResult.NotEmpty(
							anyNonSpaceDoubleChar + anyNonSpaceDoubleCharGroup + trailingWhiteSpaces
						)
				);
			}

			foreach (var nbNsDoubleInLine in _nbNsDoubleInLineCases.Value)
			{
				var trailingWhiteSpaces = withEscapedBreak || isLastLine ? anySeparateInLine : string.Empty;

				yield return new OneLineTestCase(
					testValue: anyLinePrefix + anyNonSpaceDoubleChar + nbNsDoubleInLine + anySeparateInLine +
							   lineEnding,
					result: isLastLine
						? ProcessedLineResult.LastNotEmpty(
							anyNonSpaceDoubleChar + nbNsDoubleInLine + trailingWhiteSpaces
						)
						: ProcessedLineResult.NotEmpty(
							anyNonSpaceDoubleChar + nbNsDoubleInLine + trailingWhiteSpaces
						)
				);
			}
		}

		private static IEnumerable<string> getFirstLineNegativeTestCases()
		{
			var @break = Environment.NewLine;
			var tooManyWhiteSpaces = CharStore.GetCharRange(" \t") + " ";
			var tooManyNonSpaceDoubleChars = CharStore.GetCharRange("a") + "a";
			var tooLongSeparateInLine = tooManyWhiteSpaces;

			// Without " in the beginning
			yield return $" \t\\0A \t{@break}";

			// With \ and " inside nb ns double in line
			yield return $"\" \t \\1\\0A \t{@break}";
			yield return $"\" \t \" \\0A \t{@break}";

			yield return $"\"{tooManyWhiteSpaces}\\0A \t{@break}";
			yield return $"\" \t{tooManyNonSpaceDoubleChars} \t{@break}";
			yield return $"\" \t\\0A{tooLongSeparateInLine}{@break}";

			// Without a break
			yield return "\" \t\\0A \t";
		}

		private static IEnumerable<string> getNextLineNegativeTestCases()
		{
			var @break = Environment.NewLine;
			var indent = CharStore.Spaces;
			var separateInLine = CharStore.GetCharRange(" \t");
			const string space = " ";

			var tooLongIndent = indent + space;
			var tooLongSeparateInLine = separateInLine + space;
			var tooManyNonSpaceDoubleChars = CharStore.GetCharRange("a") + "a";
			var tooManyWhiteSpaces = tooLongSeparateInLine;

			foreach (var lineEnding in new[] { @break, "\"" })
			{
				// Invalid empty lines
				yield return $"{indent}{tooLongSeparateInLine}{lineEnding}";
				yield return $"{tooLongIndent}{separateInLine}{lineEnding}";

				// Invalid non empty lines
				// With \ and " inside nb ns double in line
				yield return $" \t \\1\\0A \t{lineEnding}";
				yield return $" \t \" \\0A \t{lineEnding}";

				yield return $"\"{tooManyWhiteSpaces}\\0A \t{lineEnding}";
				yield return $"\" \t{tooManyNonSpaceDoubleChars} \t{lineEnding}";
				yield return $"\" \t\\0A{tooLongSeparateInLine}{lineEnding}";
			}

			// Invalid empty last line
			yield return $"{string.Empty}";

			// Invalid non empty last line
			yield return "\" \t\\0A \t";
		}
	}
}