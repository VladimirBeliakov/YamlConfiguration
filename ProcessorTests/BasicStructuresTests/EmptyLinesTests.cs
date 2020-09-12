using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Processor;
using Processor.TypeDefinitions;

namespace ProcessorTests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class EmptyLinesTests
	{
		[TestCaseSource(nameof(getBlockFlowWithCorrespondingRegex))]
		public void EmptyLine_ReturnsCorrespondingRegexForBlockFlow(BlockFlow value, string expectedRegex)
		{
			var actualRegex = BasicStructures.EmptyLine(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		[TestCaseSource(nameof(getBlockTestCases))]
		public void EmptyLine_Blocks_SpacesAndTabsAtBeginningAndLineBreak_Matches(BlockFlowTestCase testCase)
		{
			var regex = _emptyLineBlockFlowRegexByType[testCase.Type];

			var match = regex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[TestCaseSource(nameof(getFlowTestCases))]
		public void EmptyLine_Flows_SpacesAndTabsAtBeginningAndLineBreak_Matches(BlockFlowTestCase testCase)
		{
			var regex = _emptyLineBlockFlowRegexByType[testCase.Type];

			var match = regex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[Test]
		public void EmptyLine_NoSpacesAtBeginning_DoesNotMatch(
			[ValueSource(nameof(getBlocksAndFlows))] BlockFlow type,
			[ValueSource(nameof(getNonMatchableCases))] string testValue
		)
		{
			var regex = _emptyLineBlockFlowRegexByType[type];

			var match = regex.Match(testValue);

			Assert.False(match.Success);
		}

		private static IEnumerable<TestCaseData> getBlockFlowWithCorrespondingRegex()
		{
			var newLine = Environment.NewLine;
			foreach (var value in EnumCache.GetBlockAndFlowTypes())
			{
				switch (value)
				{
					case BlockFlow.BlockOut:
					case BlockFlow.BlockIn:
						yield return new TestCaseData(value, "^ {0,100}" + newLine);
						break;
					case BlockFlow.FlowOut:
					case BlockFlow.FlowIn:
						yield return new TestCaseData(value, "^ {0,100}(?:^|[ \t]{1,100})?" + newLine);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getCommonTestCases(BlockFlow type)
		{
			var spaces = CharStore.Spaces;
			var newLine = Environment.NewLine;

			yield return new BlockFlowTestCase(
				type, 
				testValue: String.Empty + newLine + "\tABC\t  ",
				wholeCapture: String.Empty + newLine
			);
			yield return new BlockFlowTestCase(
				type, 
				testValue: spaces + newLine + "\tABC\t  ",
				wholeCapture: spaces + newLine
			);
		}

		private static IEnumerable<BlockFlowTestCase> getBlockTestCases()
		{
			return EnumCache.GetBlockTypes().SelectMany(getCommonTestCases);
		}

		private static IEnumerable<BlockFlowTestCase> getFlowTestCases()
		{
			var oneHundredSpaces = CharStore.Spaces;
			var oneHundredSpacesAndTabs = CharStore.SpacesAndTabs;
			var @break = Environment.NewLine;

			foreach (var type in EnumCache.GetFlowTypes())
			{
				foreach (var testCase in getCommonTestCases(type))
					yield return testCase;

				yield return new BlockFlowTestCase(
					type,
					testValue: oneHundredSpaces + oneHundredSpacesAndTabs + @break +
							   "\t ABC\t  ",
					wholeCapture: oneHundredSpaces + oneHundredSpacesAndTabs + @break
				);
			}
		}

		private static IEnumerable<string> getNonMatchableCases()
		{
			var @break = Environment.NewLine;
			yield return $"ABC  {@break}  ";
			yield return $"ABC\t{@break}\t";
		}

		private static IEnumerable<BlockFlow> getBlocksAndFlows()
		{
			return EnumCache.GetBlockAndFlowTypes();
		}

		private static readonly IReadOnlyDictionary<BlockFlow, Regex> _emptyLineBlockFlowRegexByType =
			EnumCache.GetBlockAndFlowTypes().ToDictionary(
				i => i,
				i => new Regex(BasicStructures.EmptyLine(i), RegexOptions.Compiled)
			);
	}
}