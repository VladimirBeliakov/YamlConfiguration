using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using YamlConfiguration.Processor.FlowStyles;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture]
	public class SingleQuotedOneLineTests
	{
		[TestCaseSource(nameof(getPositiveOneLineTestCases))]
		public void ValidOneSingleQuotedLine_Matches((RegexTestCase, Context) testCaseWithContext)
		{
			var (testCase, context) = testCaseWithContext;

			var match = getRegexFor(context).Match(testCase.TestValue);

			Assert.That(match.Groups.Count, Is.EqualTo(2));
			Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
			Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.WholeMatch));
		}

		[TestCaseSource(nameof(getNegativeOneLineCases))]
		public void InvalidOneSingleQuotedLine_DoesNotMatch((string value, Context context) testCase)
		{
			Assert.False(getRegexFor(testCase.context, withAnchorAtEnd: true).IsMatch(testCase.value));
		}

		private static IEnumerable<(RegexTestCase, Context)> getPositiveOneLineTestCases()
		{
			static string getCharsAtEnd(Context context) => context switch
			{
				Context.BlockKey or Context.FlowKey => $"'{CharStore.Chars}",
				_ => throw new ArgumentOutOfRangeException(
						nameof(context),
						context,
						$"Only {Context.BlockKey}, {Context.FlowKey}, " +
						$"{Context.FlowIn} and {Context.FlowOut} are supported."
					),
			};

			var nbSingleOneLines =
				CharStore.NbNsSingleCharsWithoutSurrogates.Value
					.GroupBy(Characters.CharGroupMaxLength)
					.Concat(CharStore.SurrogatePairs.Value.GroupBy(Characters.CharGroupMaxLength))
					.Append(CharStore.GetCharRange("''"))
					.Append(String.Empty);

			var contexts = new[] { Context.BlockKey, Context.FlowKey };

			foreach (var nbSingleOneLine in nbSingleOneLines)
				foreach (var context in contexts)
					yield return (
							new(testValue: $"'{nbSingleOneLine}{getCharsAtEnd(context)}", wholeMatch: nbSingleOneLine),
							context
						);
		}

		private static IEnumerable<(string, Context)> getNegativeOneLineCases()
		{
			static string getLastChars(Context context) => context switch
			{
				Context.FlowKey or Context.BlockKey => "'",
				_ => throw new ArgumentOutOfRangeException(
						$"Only {Context.BlockKey}, {Context.FlowKey}, " +
						$"{Context.FlowIn} and {Context.FlowOut} are supported."
				),
			};

			var chars = CharStore.Chars;
			var tooManyNbSingleChars = CharStore.GetCharRange("a") + "a";

			var contexts = new[] { Context.FlowKey, Context.BlockKey };

			foreach (var context in contexts)
			{
				var lastChars = getLastChars(context);

				yield return ($"'\u0019{lastChars}", context);
				yield return ($"'a{chars}", context);
				yield return ($"a'{chars}", context);
				yield return ($"a{chars}", context);
				yield return ($"'{tooManyNbSingleChars}{lastChars}", context);
			}
		}

		private static Regex getRegexFor(Context context, bool withAnchorAtEnd = false)
		{
			var regexPattern = SingleQuotedStyle.GetInLinePatternFor(context);

			if (withAnchorAtEnd)
				regexPattern = regexPattern.WithAnchorAtEnd();

			return new(regexPattern);
		}
	}
}
