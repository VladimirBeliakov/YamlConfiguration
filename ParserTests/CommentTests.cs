using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Parser;

namespace ParserTests
{
	[TestFixture]
	public class CommentTests
	{
		[TestCaseSource(nameof(getTestCases))]
		public void LineWithComment_Matches(string testCase, string wholeMatch)
		{
			var match = _commentRegex.Match(testCase);

			Assert.That(match.Value, Is.EqualTo(wholeMatch));
		}

		[TestCaseSource(nameof(getUnmatchableTestCases))]
		public void NoEmptySpaceBeforeCommentIndicator_DoesNotMatch(string testValue)
		{
			var match = _commentRegex.Match(testValue);

			Assert.False(match.Success);
		}

		private static IEnumerable<TestCaseData> getTestCases()
		{
			var @break = Environment.NewLine;

			foreach (var separateInLine in CharCache.SeparateInLineCases)
			{
				yield return new TestCaseData(
/* test value */	"#" + @break,
/* whole match */	"#"
				);
				yield return new TestCaseData(
/* test value */	"#ABC" + @break,
/* whole match */	"#ABC"
				);
				yield return new TestCaseData(
/* test value */	separateInLine + "#" + @break,
/* whole match */	separateInLine + "#"
				);
				yield return new TestCaseData(
/* test value */	separateInLine + "#ABC" + @break,
/* whole match */	separateInLine + "#ABC"
				);
				yield return new TestCaseData(
/* test value */	"ABC" + separateInLine + "#" + @break,
/* whole match */	separateInLine + "#"
				);
				yield return new TestCaseData(
/* test value */	"ABC" + separateInLine + "#ABC" + @break,
/* whole match */	separateInLine + "#ABC"
				);
			}
		}

		private static IEnumerable<string> getUnmatchableTestCases()
		{
			yield return "ABC#";
			yield return "ABC#ABC";
		}

		private readonly Regex _commentRegex = new Regex(GlobalConstants.CommentRegex, RegexOptions.Compiled);
	}
}