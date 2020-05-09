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
		[Test]
		public void BreakAsSpace_FoldableLines_Matches()
		{
			var newLine = Environment.NewLine;
			var testValue = "ABC" + newLine + "ABC";
			var wholeCapture = newLine;
				
			var match = _breakAsSpaceRegex.Match(testValue);

			Assert.That(match.Value, Is.EqualTo(wholeCapture));
		}

		[TestCaseSource(nameof(getUnmatchableBlockTestCases))]
		public void BreakAsSpace_NonfoldableLine_DoesNotMatch(string unmatchableLine)
		{
			var match = _breakAsSpaceRegex.Match(unmatchableLine);

			Assert.False(match.Success);
		}

		private static IEnumerable<string> getUnmatchableBlockTestCases()
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