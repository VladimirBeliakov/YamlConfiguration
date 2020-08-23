using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Processor;

namespace ProcessorTests
{
	// TODO: Leaving the test for informational purposes only. Will be deleted when the BreakAsSpace logic
	// is moved to upper levels
	[TestFixture, Parallelizable(ParallelScope.All), Explicit]
	public class BreakAsSpaceTests
	{
		[TestCaseSource(nameof(getMatchableTestCases))]
		public void FoldableLines_Matches(string testValue, string wholeCapture)
		{
			var match = _breakAsSpaceRegex.Match(testValue);

			Assert.That(match.Value, Is.EqualTo(wholeCapture));
		}

		[TestCaseSource(nameof(getUnmatchableTestCases))]
		public void NonfoldableLine_DoesNotMatch(string unmatchableLine)
		{
			var match = _breakAsSpaceRegex.Match(unmatchableLine);

			Assert.False(match.Success);
		}

		private static IEnumerable<TestCaseData> getMatchableTestCases()
		{
			var @break = Environment.NewLine;

			yield return new TestCaseData(
/* testValue */		@break +
					"A" + @break,
/* wholeCapture */	@break
			);
			yield return new TestCaseData(
/* testValue */		"A" + @break +
					"A" + @break,
/* wholeCapture */	@break
			);
		}

		private static IEnumerable<string> getUnmatchableTestCases()
		{
			var @break = Environment.NewLine;

			yield return @break +
						 CharCache.Spaces + @break;
			yield return "ABC" + @break +
						 CharCache.Spaces + @break;
			yield return "ABC" + @break +
						 CharCache.Tabs + @break;
			yield return "ABC" + @break +
						 CharCache.SpacesAndTabs + @break;
		}

		private static readonly Regex _breakAsSpaceRegex = new Regex(
			BasicStructures.BreakAsSpace(),
			RegexOptions.Compiled
		);
	}
}