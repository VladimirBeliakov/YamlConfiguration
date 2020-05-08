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
	public class EmptyLinesTests
	{
		[TestCaseSource(nameof(getEmptyLineBlockFlowWithCorrespondingRegex))]
		public void EmptyLine_ReturnsCorrespondingRegexForBlockFlow(BlockFlowInOut value, string expectedRegex)
		{
			var actualRegex = GlobalConstants.EmptyLine(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		[TestCaseSource(nameof(getEmptyLineBlockTestCases))]
		public void EmptyLine_Blocks_SpacesAndTabsAtBeginningAndLineBreak_Matches(BlockFlowTestCase testCase)
		{
			var regex = _emptyLineBlockFlowRegexByType[testCase.Type];

			var matches = regex.Matches(testCase.Value);

			Assert.That(matches.Count, Is.EqualTo(1));
			Assert.Multiple(() =>
			{
				Assert.That(matches[0].Groups.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
			});
		}

		[TestCaseSource(nameof(getEmptyLineFlowTestCases))]
		public void EmptyLine_Flows_SpacesAndTabsAtBeginningAndLineBreak_Matches(BlockFlowTestCase testCase)
		{
			var regex = _emptyLineBlockFlowRegexByType[testCase.Type];

			var matches = regex.Matches(testCase.Value);

			Assert.That(matches.Count, Is.EqualTo(1));
			Assert.Multiple(() =>
			{
				Assert.That(matches[0].Groups.Count, Is.EqualTo(2));
				Assert.That(matches[0].Groups[0].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
				if (testCase.FirstParenthesisCapture is null)
				{
					Assert.That(matches[0].Groups[1].Value, Is.Empty);
				}
				else
				{
					Assert.That(matches[0].Groups[1].Captures.Count, Is.EqualTo(1));
					Assert.That(matches[0].Groups[1].Captures[0].Value, Is.EqualTo(testCase.FirstParenthesisCapture));
				}
			});
		}

		[Test]
		public void EmptyLine_NoSpacesAtBeginning_DoesNotMatch(
			[ValueSource(nameof(GetBlocksAndFlows))] BlockFlowInOut type,
			[ValueSource(nameof(getEmptyLineNonMatchableCases))] string testValue
		)
		{
			var regex = _emptyLineBlockFlowRegexByType[type];

			var match = regex.Match(testValue);

			Assert.False(match.Success);
		}

		private static IEnumerable<TestCaseData> getEmptyLineBlockFlowWithCorrespondingRegex()
		{
			var newLine = Environment.NewLine;
			foreach (var value in BlockFlowCache.GetBlockAndFlowTypes())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(value, "^ {0,100}" + newLine);
						break;
					case BlockFlowInOut.FlowOut:
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(value, "^ {0,100}([ \t]{1,100})?" + newLine);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getEmptyLineBlockTestCases()
		{
			var oneHundredSpaces = String.Join(String.Empty, Enumerable.Repeat(' ', 100));
			var newLine = Environment.NewLine;

			foreach (var type in BlockFlowCache.GetBlockTypes())
			{
				yield return new BlockFlowTestCase(
					type, 
					value: String.Empty + newLine + "\tABC\t  ",
					wholeCapture: String.Empty + newLine
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + newLine + "\tABC\t  ",
					wholeCapture: oneHundredSpaces + newLine
				);
			}
		}

		private static IEnumerable<BlockFlowTestCase> getEmptyLineFlowTestCases()
		{
			var oneHundredSpaces = String.Join(String.Empty, Enumerable.Repeat(' ', 100));
			var oneHundredSpacesAndTabs = String.Join(String.Empty, Enumerable.Repeat("\t ", 50));
			var newLine = Environment.NewLine;

			foreach (var type in BlockFlowCache.GetFlowTypes())
			{
				yield return new BlockFlowTestCase(
					type, 
					value: String.Empty + newLine + "ABC\t  ",
					wholeCapture: String.Empty + newLine
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + newLine + "ABC\t  ",
					wholeCapture: oneHundredSpaces + newLine
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + oneHundredSpacesAndTabs + newLine + "\t ABC\t  ",
					wholeCapture: oneHundredSpaces + oneHundredSpacesAndTabs + newLine,
					firstParenthesisCapture: oneHundredSpacesAndTabs
				);
			}
		}

		private static IEnumerable<string> getEmptyLineNonMatchableCases()
		{
			var newLine = Environment.NewLine;
			yield return $"ABC  {newLine}  ";
			yield return $"ABC\t{newLine}\t";
		}

		private static IEnumerable<BlockFlowInOut> GetBlocksAndFlows()
		{
			return BlockFlowCache.GetBlockAndFlowTypes();
		}

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _emptyLineBlockFlowRegexByType =
			BlockFlowCache.GetBlockAndFlowTypes().ToDictionary(
				i => i,
				i => new Regex(GlobalConstants.EmptyLine(i), RegexOptions.Compiled)
			);
	}
}