using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Parser;

namespace ParserTests
{
	[TestFixture, Parallelizable(ParallelScope.Children)]
	public class BreakAsSpaceTests
	{
		[TestCaseSource(nameof(getMatchableTestCases))]
		public void BreakAsSpace_FoldableLines_Matches(string testValue, string wholeCapture)
		{
			var match = _breakAsSpaceRegex.Match(testValue);

			Assert.That(match.Value, Is.EqualTo(wholeCapture));
		}

		[TestCaseSource(nameof(getUnmatchableTestCases))]
		public void BreakAsSpace_NonfoldableLine_DoesNotMatch(string unmatchableLine)
		{
			var match = _breakAsSpaceRegex.Match(unmatchableLine);

			Assert.False(match.Success);
		}

		private static IEnumerable<TestCaseData> getMatchableTestCases()
		{
			var chars = CharCache.Chars;
			var newLine = Environment.NewLine;
			var spacesAndTabs = CharCache.SpacesAndTabs;

			yield return new TestCaseData(
/* testValue */		"A" + newLine +
					"A",
/* wholeCapture */	"A" + newLine
			);
			yield return new TestCaseData(
/* testValue */		spacesAndTabs + chars + spacesAndTabs + newLine +
					spacesAndTabs + chars + spacesAndTabs,
/* wholeCapture */	spacesAndTabs + chars + spacesAndTabs + newLine
			);
		}

		private static IEnumerable<string> getUnmatchableTestCases()
		{
			var newLine = Environment.NewLine;

			yield return "ABC" + newLine +
						 CharCache.Spaces;
			yield return "ABC" + newLine +
						 CharCache.Tabs;
			yield return "ABC" + newLine +
						 CharCache.SpacesAndTabs;
		}

		private static readonly Regex _breakAsSpaceRegex = new Regex(GlobalConstants.BreakAsSpace);
	}
}