using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Processor;
using Processor.TypeDefinitions;

namespace ProcessorTests
{
	// TODO: Leaving the test for informational purposes only. Will be deleted when the TrimmedLine
	// logic is moved to upper levels
	[TestFixture, Parallelizable(ParallelScope.All), Explicit]
	public class TrimmedLineTests
	{
		[TestCaseSource(nameof(getTrimmedLineBlockFlowWithCorrespondingRegex))]
		public void ReturnsCorrespondingRegexForBlockOrFlowType(BlockFlow value, string expectedRegex)
		{
			var actualRegex = BasicStructures.TrimmedLine(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		[TestCaseSource(nameof(getTrimmedLineBlockTestCases))]
		public void Blocks_LineBreakAndEmptyLine_Matches(BlockFlowTestCase testCase)
		{
			var regex = _trimmedLineRegex[testCase.Type];

			var match = regex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[TestCaseSource(nameof(getTrimmedLineFlowTestCases))]
		public void Flows_LineBreakAndEmptyLine_Matches(BlockFlowTestCase testCase)
		{
			var regex = _trimmedLineRegex[testCase.Type];

			var match = regex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[Test]
		public void NoEmptyLines_DoesNotMatch(
			[ValueSource(nameof(getBlocksAndFlows))] BlockFlow type,
			[ValueSource(nameof(getTrimmedLineNonMatchableCases))] string testValue
		)
		{
			var regex = _trimmedLineRegex[type];

			var match = regex.Match(testValue);

			Assert.False(match.Success);
		}

		private static IEnumerable<TestCaseData> getTrimmedLineBlockFlowWithCorrespondingRegex()
		{
			var newLine = Environment.NewLine;
			var charGroupLength = Characters.CharGroupLength;

			foreach (var value in EnumCache.GetBlockAndFlowTypes())
			{
				switch (value)
				{
					case BlockFlow.BlockOut:
					case BlockFlow.BlockIn:
						yield return new TestCaseData(
							value,
							newLine +
							$"(?: {{0,{charGroupLength}}}" + newLine + ")+"
						);
						break;
					case BlockFlow.FlowOut:
					case BlockFlow.FlowIn:
						yield return new TestCaseData(
							value,
							newLine +
							$"(?: {{0,{charGroupLength}}}" + $"(?:^|[ \t]{{1,{charGroupLength}}})?" + newLine + ")+"
						);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineCommonTestCases(BlockFlow type)
		{
			var spaces = CharStore.Spaces;
			var @break = Environment.NewLine;

			yield return new BlockFlowTestCase(
				type,
				testValue: @break +
						   @break +
						   "\tABC\t  ",
				wholeCapture: @break +
							  @break
			);
			yield return new BlockFlowTestCase(
				type,
				testValue: "\tABC\t  " + @break +
						   @break +
						   "\tABC\t  ",
				wholeCapture: @break +
							  @break
			);
			yield return new BlockFlowTestCase(
				type,
				testValue: "\tABC\t  " + @break +
						   spaces + @break +
						   "\tABC\t  ",
				wholeCapture: @break +
							  spaces + @break
			);
			yield return new BlockFlowTestCase(
				type,
				testValue: "\tABC\t  " + @break +
						   spaces + @break +
						   spaces + @break +
						   "\tABC\t  ",
				wholeCapture: @break +
							  spaces + @break +
							  spaces + @break
			);
		}
		
		private static IEnumerable<BlockFlowTestCase> getTrimmedLineBlockTestCases()
		{
			return EnumCache.GetBlockTypes().SelectMany(getTrimmedLineCommonTestCases);
		}

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineFlowTestCases()
		{
			var spaces = CharStore.Spaces;
			var spacesAndTabs = CharStore.SpacesAndTabs;
			var newLine = Environment.NewLine;

			foreach (var type in EnumCache.GetFlowTypes())
			{
				foreach (var testCase in getTrimmedLineCommonTestCases(type))
					yield return testCase;

				yield return new BlockFlowTestCase(
					type,
					testValue: newLine +
							   spacesAndTabs + newLine +
							   "\tABC\t  ",
					wholeCapture: newLine +
								  spacesAndTabs + newLine
				);
				yield return new BlockFlowTestCase(
					type,
					testValue: "\tABC\t  " + newLine + 
							   spacesAndTabs + newLine + 
							   "\tABC\t  ",
					wholeCapture: newLine +
								  spacesAndTabs + newLine
				);
				yield return new BlockFlowTestCase(
					type,
					testValue: "\tABC\t  " + newLine + 
							   spaces + spacesAndTabs + newLine + 
							   "\tABC\t  ",
					wholeCapture: newLine +
								  spaces + spacesAndTabs + newLine
				);
				yield return new BlockFlowTestCase(
					type,
					testValue: "\tABC\t  " + newLine + 
							   spaces + spacesAndTabs + newLine + 
							   spaces + spacesAndTabs + newLine + 
							   "\tABC\t  ",
					wholeCapture: newLine +
								  spaces + spacesAndTabs + newLine + 
								  spaces + spacesAndTabs + newLine
				);
			}
		}

		private static IEnumerable<string> getTrimmedLineNonMatchableCases()
		{
			var @break = Environment.NewLine;
			yield return $"{@break}  ABC";
			yield return $"ABC  {@break}  ABC";
			yield return $"ABC\t{@break}\tABC";
		}

		private static IEnumerable<BlockFlow> getBlocksAndFlows()
		{
			return EnumCache.GetBlockAndFlowTypes();
		}

		private static readonly IReadOnlyDictionary<BlockFlow, Regex> _trimmedLineRegex =
			EnumCache.GetBlockAndFlowTypes().ToDictionary(
				i => i,
				i => new Regex(BasicStructures.TrimmedLine(i), RegexOptions.Compiled)
			);
	}
}