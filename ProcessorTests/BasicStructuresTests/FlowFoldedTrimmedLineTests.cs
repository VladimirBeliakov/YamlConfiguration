using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Processor;
using Processor.TypeDefinitions;

namespace ProcessorTests
{
	// TODO: Leaving the test for informational purposes only. Will be deleted when the FlowFoldedTrimmedLine logic
	// is moved to upper levels
	[TestFixture, Parallelizable(ParallelScope.Children), Explicit]
	public class FlowFoldedTrimmedLineTests
	{
		[TestCaseSource(nameof(getTestCases))]
		public void FoldableTrimmableLine_Matches(BlockFlowTestCase testCase)
		{
			var match = _flowFoldedTrimmedLineRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[TestCaseSource(nameof(getUnmatchableTestCases))]
		public void UnfoldableUntrimmableLine_DoesNotMatch(string testValue)
		{
			var match = _flowFoldedTrimmedLineRegex.Match(testValue);

			Assert.False(match.Success);
		}

		private static IEnumerable<BlockFlowTestCase> getTestCases()
		{
			var spaces = CharCache.Spaces;
			var @break = Environment.NewLine;

			foreach (var separateInLine in new[] { String.Empty }.Concat(CharCache.SeparateInLineCases))
			{
				foreach (var linePrefix in new[] { String.Empty, spaces + separateInLine })
				{
					foreach (var trimmedLine in new[]
					{
						@break +
						linePrefix + @break,
						@break +
						linePrefix + @break +
						linePrefix + @break,
					})
					{
						yield return new BlockFlowTestCase(
							BlockFlow.FlowIn,
							testValue: separateInLine +
									   trimmedLine +
									   linePrefix + "ABC",
							wholeCapture: separateInLine +
										  trimmedLine +
										  linePrefix
						);
						yield return new BlockFlowTestCase(
							BlockFlow.FlowIn,
							testValue: "ABC" + separateInLine +
									   trimmedLine +
									   linePrefix + "ABC",
							wholeCapture: separateInLine +
										  trimmedLine +
										  linePrefix
						);
					}
				}
			}
		}

		private static IEnumerable<string> getUnmatchableTestCases()
		{
			var @break = Environment.NewLine;
			yield return $"{@break}  ABC";
			yield return $"ABC  {@break}  ABC";
			yield return $"ABC\t{@break}\tABC";
		}

		private readonly Regex _flowFoldedTrimmedLineRegex = new Regex(
			BasicStructures.FlowFoldedTrimmedLine(),
			RegexOptions.Compiled
		);
	}
}