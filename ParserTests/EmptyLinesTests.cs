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
			[Values("ABC   ", "\tABC   ")] string testValue
		)
		{
			var regex = _emptyLineBlockFlowRegexByType[type];

			var matches = regex.Matches(testValue);

			Assert.That(matches.Count, Is.EqualTo(0));
		}

		private static IEnumerable<TestCaseData> getEmptyLineBlockFlowWithCorrespondingRegex()
		{
			var emptyLinePostfix = "(\r\n?|\n)";
			foreach (var value in BlockFlowCache.GetBlocksAndFlows())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(value, "^ {1,100}" + emptyLinePostfix);
						break;
					case BlockFlowInOut.FlowOut:
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(value, "^ {1,100}([ \t]{1,100})?" + emptyLinePostfix);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getEmptyLineBlockTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var lineBreaks = new[] { "\r\n", "\r", "\n" };

			foreach (var lineBreak in lineBreaks)
			{
				foreach (var type in BlockFlowCache.GetBlockTypes())
				{
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + lineBreak + "\tABC\t  ",
						wholeCapture: oneHundredSpaces + lineBreak,
						firstParenthesisCapture: lineBreak
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + lineBreak + " ABC\t  ",
						wholeCapture: oneHundredSpaces + lineBreak,
						firstParenthesisCapture: lineBreak
					);
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getEmptyLineFlowTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var oneHundredSpacesAndTabs =
				new String(Enumerable.Repeat('\t', 50).Concat(Enumerable.Repeat(' ', 50)).ToArray());
			var lineBreaks = new[] { "\r\n", "\r", "\n" };

			foreach (var lineBreak in lineBreaks)
			{
				foreach (var type in BlockFlowCache.GetFlowTypes())
				{
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + "\t" + lineBreak + "ABC\t  ",
						wholeCapture: oneHundredSpaces + "\t" + lineBreak,
						firstParenthesisCapture: "\t",
						secondParenthesisCapture: lineBreak
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + " " + lineBreak + "ABC\t  ",
						wholeCapture: oneHundredSpaces + " " + lineBreak,
						firstParenthesisCapture: " ",
						secondParenthesisCapture: lineBreak
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + " \t" + lineBreak + "ABC\t  ",
						wholeCapture: oneHundredSpaces + " \t" + lineBreak,
						firstParenthesisCapture: " \t",
						secondParenthesisCapture: lineBreak
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + "\t " + lineBreak + "ABC\t  ",
						wholeCapture: oneHundredSpaces + "\t " + lineBreak,
						firstParenthesisCapture: "\t ",
						secondParenthesisCapture: lineBreak
					);
					yield return new BlockFlowTestCase(
						type, 
						value: oneHundredSpaces + oneHundredSpacesAndTabs + lineBreak + "\t ABC\t  ",
						wholeCapture: oneHundredSpaces + oneHundredSpacesAndTabs + lineBreak,
						firstParenthesisCapture: oneHundredSpacesAndTabs,
						secondParenthesisCapture: lineBreak
					);
				}
			}
		}

		private static IEnumerable<BlockFlowInOut> GetBlocksAndFlows()
		{
			return BlockFlowCache.GetBlocksAndFlows();
		}

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _emptyLineBlockFlowRegexByType =
			BlockFlowCache.GetBlocksAndFlows().ToDictionary(
				i => i,
				i => new Regex(GlobalConstants.EmptyLine(i), RegexOptions.Compiled)
			);
	}
}