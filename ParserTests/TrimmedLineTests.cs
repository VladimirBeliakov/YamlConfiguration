using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Parser;
using Parser.TypeDefinitions;

namespace ParserTests
{
	[TestFixture, Parallelizable(ParallelScope.Children)]
	public class TrimmedLineTests
	{
		[TestCaseSource(nameof(getTrimmedLineBlockFlowWithCorrespondingRegex))]
		public void TrimmedLine_ReturnsCorrespondingRegexForBlockOrFlowType(BlockFlowInOut value, string expectedRegex)
		{
			var actualRegex = GlobalConstants.TrimmedLine(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		[TestCaseSource(nameof(getTrimmedLineBlockTestCases))]
		public void TrimmedLine_Blocks_LineBreakAndEmptyLine_Matches(BlockFlowTestCase testCase)
		{
			var regex = _trimmedLineRegex[testCase.Type];

			var match = regex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[TestCaseSource(nameof(getTrimmedLineFlowTestCases))]
		public void TrimmedLine_Flows_LineBreakAndEmptyLine_Matches(BlockFlowTestCase testCase)
		{
			var regex = _trimmedLineRegex[testCase.Type];

			var match = regex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[Test]
		public void TrimmedLine_NoEmptyLines_DoesNotMatch(
			[ValueSource(nameof(getBlocksAndFlows))] BlockFlowInOut type,
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
			var charGroupLength = GlobalConstants.CharGroupLength;
			var notEmptyLine = 
				$"^(?:.{{0,{charGroupLength}}}" + $"[^ \t]{{1,{charGroupLength}}}" + $".{{0,{charGroupLength}}})";

			foreach (var value in EnumCache.GetBlockAndFlowTypes())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(
							value,
							notEmptyLine + newLine + 
							$"(?: {{0,{charGroupLength}}}" + newLine + ")+"
						);
						break;
					case BlockFlowInOut.FlowOut:
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(
							value,
							notEmptyLine + newLine + 
							$"(?: {{0,{charGroupLength}}}" + $"(?:[ \t]{{1,{charGroupLength}}})?" + newLine + ")+"
						);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineCommonTestCases(BlockFlowInOut type)
		{
			var chars = CharCache.Chars;
			var spaces = CharCache.Spaces;
			var newLine = Environment.NewLine;
			var spacesAndTabs = CharCache.SpacesAndTabs;

			yield return new BlockFlowTestCase(
				type,
				testValue: "A" + newLine + 
						   newLine + 
						   "\tABC\t  ",
				wholeCapture: "A" + newLine + 
							  newLine
			);
			yield return new BlockFlowTestCase(
				type,
				testValue: spacesAndTabs + chars + spacesAndTabs + newLine + 
						   newLine + 
						   "\tABC\t  ",
				wholeCapture: spacesAndTabs + chars + spacesAndTabs + newLine + 
							  newLine
			);
			yield return new BlockFlowTestCase(
				type,
				testValue: "\tABC\t  " + newLine + 
						   newLine + 
						   "\tABC\t  ",
				wholeCapture: "\tABC\t  " + newLine +
							  newLine
			);
			yield return new BlockFlowTestCase(
				type,
				testValue: "\tABC\t  " + newLine + 
						   spaces + newLine + 
						   "\tABC\t  ",
				wholeCapture: "\tABC\t  " + newLine + 
							  spaces + newLine
			);
			yield return new BlockFlowTestCase(
				type,
				testValue: "\tABC\t  " + newLine + 
						   spaces + newLine + 
						   spaces + newLine + 
						   "\tABC\t  ",
				wholeCapture: "\tABC\t  " + newLine + 
							  spaces + newLine + 
							  spaces + newLine
			);
		}
		
		private static IEnumerable<BlockFlowTestCase> getTrimmedLineBlockTestCases()
		{
			return EnumCache.GetBlockTypes().SelectMany(getTrimmedLineCommonTestCases);
		}

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineFlowTestCases()
		{
			var spaces = CharCache.Spaces;
			var spacesAndTabs = CharCache.SpacesAndTabs;
			var newLine = Environment.NewLine;

			foreach (var type in EnumCache.GetFlowTypes())
			{
				foreach (var testCase in getTrimmedLineCommonTestCases(type))
					yield return testCase;

				yield return new BlockFlowTestCase(
					type,
					testValue: "\tABC\t  " + newLine + 
							   spacesAndTabs + newLine + 
							   "\tABC\t  ",
					wholeCapture: "\tABC\t  " + newLine + 
								  spacesAndTabs + newLine
				);
				yield return new BlockFlowTestCase(
					type,
					testValue: "\tABC\t  " + newLine + 
							   spaces + spacesAndTabs + newLine + 
							   "\tABC\t  ",
					wholeCapture: "\tABC\t  " + newLine + 
								  spaces + spacesAndTabs + newLine
				);
				yield return new BlockFlowTestCase(
					type,
					testValue: "\tABC\t  " + newLine + 
							   spaces + spacesAndTabs + newLine + 
							   spaces + spacesAndTabs + newLine + 
							   "\tABC\t  ",
					wholeCapture: "\tABC\t  " + newLine + 
								  spaces + spacesAndTabs + newLine + 
								  spaces + spacesAndTabs + newLine
				);
			}
		}

		private static IEnumerable<string> getTrimmedLineNonMatchableCases()
		{
			var newLine = Environment.NewLine;
			yield return $"ABC  {newLine}  ABC";
			yield return $"ABC\t{newLine}\tABC";
		}

		private static IEnumerable<BlockFlowInOut> getBlocksAndFlows()
		{
			return EnumCache.GetBlockAndFlowTypes();
		}

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _trimmedLineRegex =
			EnumCache.GetBlockAndFlowTypes().ToDictionary(
				i => i,
				i => new Regex(GlobalConstants.TrimmedLine(i), RegexOptions.Compiled)
			);
	}
}