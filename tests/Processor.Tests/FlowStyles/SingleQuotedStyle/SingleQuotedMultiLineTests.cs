using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class SingleQuotedMultiLineTests : QuotedMultiLineBaseTest
	{
		[TestCaseSource(nameof(getFirstLinePositiveTestCases))]
		public void ValidSingleQuotedFirstLine_Matches((RegexTestCase, Context) testCaseWithContext)
		{
			var (testCase, context) = testCaseWithContext;

			var match = getFirstLineRegexFor(context).Match(testCase.TestValue);

			assert(testCase, match);
		}

		[TestCaseSource(nameof(getNextLinePositiveTestCases))]
		public void ValidSingleQuotedNextLine_Matches((RegexTestCase, Context) testCaseWithContext)
		{
			var (testCase, context) = testCaseWithContext;

			var match = getNextLineRegexFor(context).Match(testCase.TestValue);

			assert(testCase, match);
		}

		[TestCaseSource(nameof(getFirstLineNegativeTestCases))]
		public void InvalidSingleQuotedFirstLine_DoesNotMatch((string, Context) testCaseWithContext)
		{
			var (testCase, context) = testCaseWithContext;

			var result = getFirstLineRegexFor(context, withAnchorAtEnd: true).IsMatch(testCase);

			Assert.False(result);
		}

		[TestCaseSource(nameof(getNextLineNegativeTestCases))]
		public void InvalidSingleQuotedNextLine_DoesNotMatch((string, Context) testCaseWithContext)
		{
			var (testCase, context) = testCaseWithContext;

			var result = getNextLineRegexFor(context, withAnchorAtEnd: true).IsMatch(testCase);

			Assert.False(result);
		}

		private static void assert(RegexTestCase testCase, Match match)
		{
			Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));

			Assert.That(match.Groups.Count, Is.EqualTo(4));

			var content = testCase.Captures?.FirstOrDefault();
			if (content is null)
			{
				Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(0));
				Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(0));
				Assert.That(match.Groups[3].Captures.Count, Is.EqualTo(0));
				return;
			}

			Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
			Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(content));

			var trailingWhitesAndQuote = testCase.Captures?.ElementAtOrDefault(1);
			if (trailingWhitesAndQuote is null)
			{
				Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(0));
				Assert.That(match.Groups[3].Captures.Count, Is.EqualTo(0));
				return;
			}

			Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(1));
			Assert.That(match.Groups[2].Captures[0].Value, Is.EqualTo(trailingWhitesAndQuote));

			var trailingWhites = testCase.Captures?.ElementAtOrDefault(2);
			if (trailingWhites is null)
			{
				Assert.That(match.Groups[3].Captures.Count, Is.EqualTo(0));
				return;
			}

			Assert.That(match.Groups[3].Captures.Count, Is.EqualTo(1));
			Assert.That(match.Groups[3].Captures[0].Value, Is.EqualTo(trailingWhites));
		}

		private static IEnumerable<(RegexTestCase, Context)> getFirstLinePositiveTestCases()
		{
			var firstLines = GetFirstLines(isDoubleQuoted: false, withClosingQuote: false)
				.Concat(GetFirstLines(isDoubleQuoted: false, withClosingQuote: true));

			foreach (var context in _availableContext)
				foreach (var testCase in firstLines)
					yield return (testCase, context);
		}

		private static IEnumerable<(RegexTestCase, Context)> getNextLinePositiveTestCases()
		{
			var nextLines = GetNextLines(isDoubleQuoted: false, withClosingQuote: false)
				.Concat(GetNextLines(isDoubleQuoted: false, withClosingQuote: true));

			foreach (var context in _availableContext)
				foreach (var nextLine in nextLines)
					yield return (nextLine, context);
		}

		private static IEnumerable<(string, Context)> getFirstLineNegativeTestCases()
		{
			var firstLines = GetFirstLineNegativeTestCases(isDoubleQuote: false);

			foreach (var context in _availableContext)
				foreach (var firstLine in firstLines)
					yield return (firstLine, context);
		}

		private static IEnumerable<(string, Context)> getNextLineNegativeTestCases()
		{
			var nextLines = GetNextLineNegativeTestCases(isDoubleQuote: false);

			foreach (var context in _availableContext)
				foreach (var nextLine in nextLines)
					yield return (nextLine, context);
		}

		private static Regex getFirstLineRegexFor(Context context, bool withAnchorAtEnd = false)
		{
			var regexPattern = SingleQuotedStyle.MultiLine.GetFirstLinePatternFor(context);

			if (withAnchorAtEnd)
				regexPattern = regexPattern.WithAnchorAtEnd();

			return new(regexPattern);
		}

		private static Regex getNextLineRegexFor(Context context, bool withAnchorAtEnd = false)
		{
			var regexPattern = SingleQuotedStyle.MultiLine.GetNextLinePatternFor(context);

			if (withAnchorAtEnd)
				regexPattern = regexPattern.WithAnchorAtEnd();

			return new(regexPattern);
		}

		private static readonly IReadOnlyCollection<Context> _availableContext =
			new[] { Context.FlowIn, Context.FlowOut };
	}
}
