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
				Assert.That(matches[0].Groups.Count, Is.EqualTo(2));
				Assert.That(matches[0].Groups[0].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
				Assert.That(matches[0].Groups[1].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[1].Captures[0].Value, Is.EqualTo(testCase.FirstParenthesisCapture));
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
				Assert.That(matches[0].Groups.Count, Is.EqualTo(3));
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
				if (testCase.SecondParenthesisCapture is null)
				{
					Assert.That(matches[0].Groups[2].Value, Is.Empty);
				}
				else
				{
					Assert.That(matches[0].Groups[2].Captures.Count, Is.EqualTo(1));
					Assert.That(matches[0].Groups[2].Captures[0].Value, Is.EqualTo(testCase.SecondParenthesisCapture));
				}
			});
		}

		[Test]
		public void EmptyLine_NoSpacesAtBeginning_DoesNotMatch(
			[ValueSource(nameof(GetBlocksAndFlows))] BlockFlowInOut type,
			[Values("ABC\n   ", "\tABC\n   ", "ABC\r   ", "\tABC\r   ")] string testValue
		)
		{
			var regex = _emptyLineBlockFlowRegexByType[type];

			var matches = regex.Matches(testValue);

			Assert.That(matches.Count, Is.EqualTo(0));
		}

		private static IEnumerable<TestCaseData> getEmptyLineBlockFlowWithCorrespondingRegex()
		{
			var newLine = "(\r\n?|\n)";
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
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var newLines = new[] { "\r\n", "\r", "\n" };

			foreach (var newLine in newLines)
			{
				foreach (var type in BlockFlowCache.GetBlockTypes())
				{
					yield return new BlockFlowTestCase(
						type, 
						value: String.Empty + newLine + "\tABC\t  ",
						wholeCapture: String.Empty + newLine,
						firstParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + newLine + "\tABC\t  ",
						wholeCapture: oneHundredSpaces + newLine,
						firstParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + newLine + " ABC\t  ",
						wholeCapture: oneHundredSpaces + newLine,
						firstParenthesisCapture: newLine
					);
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getEmptyLineFlowTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var oneHundredSpacesAndTabs = String.Join(String.Empty, Enumerable.Repeat("\t ", 50));
			var newLines = new[] { "\r\n", "\r", "\n" };

			foreach (var newLine in newLines)
			{
				foreach (var type in BlockFlowCache.GetFlowTypes())
				{
					yield return new BlockFlowTestCase(
						type, 
						value: String.Empty + "\t" + newLine + "ABC\t  ",
						wholeCapture: String.Empty + "\t" + newLine,
						firstParenthesisCapture: "\t",
						secondParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + "\t" + newLine + "ABC\t  ",
						wholeCapture: oneHundredSpaces + "\t" + newLine,
						firstParenthesisCapture: "\t",
						secondParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + " " + newLine + "ABC\t  ",
						wholeCapture: oneHundredSpaces + " " + newLine,
						firstParenthesisCapture: " ",
						secondParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + " \t" + newLine + "ABC\t  ",
						wholeCapture: oneHundredSpaces + " \t" + newLine,
						firstParenthesisCapture: " \t",
						secondParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + "\t " + newLine + "ABC\t  ",
						wholeCapture: oneHundredSpaces + "\t " + newLine,
						firstParenthesisCapture: "\t ",
						secondParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + oneHundredSpacesAndTabs + newLine + "\t ABC\t  ",
						wholeCapture: oneHundredSpaces + oneHundredSpacesAndTabs + newLine,
						firstParenthesisCapture: oneHundredSpacesAndTabs,
						secondParenthesisCapture: newLine
					);
				}
			}
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