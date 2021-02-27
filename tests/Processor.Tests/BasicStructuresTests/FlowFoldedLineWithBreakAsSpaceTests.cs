using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	// TODO: Leaving the test for informational purposes only. Will be deleted when the FlowFoldedLineWithBreakAsSpace
	// logic is moved to upper levels
	[TestFixture, Parallelizable(ParallelScope.All), Explicit]
	public class FlowFoldedLineWithBreakAsSpaceTests
	{
		[TestCaseSource(nameof(getTestCases))]
		public void FoldableLineWithBreakAsSpace_Matches(BlockFlowTestCase testCase)
		{
			var match = _flowFoldedLineWithBreakAsSpaceRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[TestCaseSource(nameof(getUnmatchableTestCases))]
		public void UnfoldableLineWithoutBreakAsSpace_DoesNotMatch(string testValue)
		{
			var match = _flowFoldedLineWithBreakAsSpaceRegex.Match(testValue);

			Assert.False(match.Success);
		}

		private static IEnumerable<BlockFlowTestCase> getTestCases()
		{
			var spaces = CharStore.Spaces;
			var spacesAndTabs = CharStore.SpacesAndTabs;
			var chars = CharStore.Chars;
			var @break = Environment.NewLine;

			foreach (var separateInLine in new[] { String.Empty }.Concat(CharStore.SeparateInLineCases))
			{
				foreach (var linePrefix in new[] { String.Empty, spaces + separateInLine })
				{
					foreach (var breakAsSpace in new[]
					{
						@break +
						linePrefix + "A",
						@break +
						linePrefix + spacesAndTabs + "A",
						@break +
						linePrefix + "A" + spacesAndTabs,
						@break +
						linePrefix + spacesAndTabs + "A" + spacesAndTabs,
						@break +
						linePrefix + chars,
						@break +
						linePrefix + spacesAndTabs + chars,
						@break +
						linePrefix + chars + spacesAndTabs,
						@break +
						linePrefix + spacesAndTabs + chars + spacesAndTabs,
					})
					{
						yield return new BlockFlowTestCase(
							BlockFlow.FlowIn,
							testValue: separateInLine +
									   breakAsSpace +
									   "ABC" + @break,
							wholeCapture: separateInLine +
										  @break +
										  linePrefix
						);
						yield return new BlockFlowTestCase(
							BlockFlow.FlowIn,
							testValue: "ABC" + separateInLine +
									   breakAsSpace +
									   "ABC" + @break,
							wholeCapture: separateInLine +
										  @break +
										  linePrefix
						);
					}
				}
			}
		}

		private static IEnumerable<string> getUnmatchableTestCases()
		{
			var @break = Environment.NewLine;

			yield return @break +
						 CharStore.Spaces + @break;
			yield return "ABC" + @break +
						 CharStore.Spaces + @break;
			yield return "ABC" + @break +
						 CharStore.Tabs + @break;
			yield return "ABC" + @break +
						 CharStore.SpacesAndTabs + @break;
		}

		private readonly Regex _flowFoldedLineWithBreakAsSpaceRegex = new Regex(
			BasicStructures.FlowFoldedLineWithBreakAsSpace(),
			RegexOptions.Compiled
		);
	}
}