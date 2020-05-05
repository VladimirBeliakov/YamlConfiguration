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
	public class LinePrefixTests
	{
		[TestCaseSource(nameof(getLinePrefixBlockFlowWithCorrespondingRegex))]
		public void LinePrefix_ReturnsCorrespondingRegexForBlockFlow(BlockFlowInOut value, string expectedRegex)
		{
			var actualRegex = GlobalConstants.LinePrefix(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		[TestCaseSource(nameof(getLinePrefixBlockTestCases))]
		public void LinePrefix_Blocks_SpacesAtBeginning_Matches(BlockFlowTestCase testCase)
		{
			var regex = _linePrefixBlockFlowRegexByType[testCase.Type];

			var matches = regex.Matches(testCase.Value);

			Assert.That(matches.Count, Is.EqualTo(1));
			Assert.Multiple(() =>
			{
				Assert.That(matches[0].Groups.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
			});
		}

		[TestCaseSource(nameof(getLinePrefixFlowTestCases))]
		public void LinePrefix_Flows_SpacesAndTabsAtBeginning_Matches(BlockFlowTestCase testCase)
		{
			var regex = _linePrefixBlockFlowRegexByType[testCase.Type];

			var matches = regex.Matches(testCase.Value);

			Assert.That(matches.Count, Is.EqualTo(1));
			Assert.Multiple(() =>
			{
				Assert.That(matches[0].Groups.Count, Is.EqualTo(2));
				Assert.That(matches[0].Groups[0].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
				if (String.IsNullOrEmpty(testCase.FirstParenthesisCapture))
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

		private static IEnumerable<TestCaseData> getLinePrefixBlockFlowWithCorrespondingRegex()
		{
			foreach (var value in BlockFlowCache.GetBlockAndFlowTypes())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(value, "^ {0,100}");
						break;
					case BlockFlowInOut.FlowOut:
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(value, "^ {0,100}([ \t]{1,100})?");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getLinePrefixBlockTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());

			foreach (var type in BlockFlowCache.GetBlockTypes())
			{
				yield return new BlockFlowTestCase(
					type, 
					value: String.Empty + "\tABC\t  ",
					wholeCapture: String.Empty
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + "\tABC\t  ",
					wholeCapture: oneHundredSpaces
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + " ABC\t  ",
					wholeCapture: oneHundredSpaces
				);
			}
		}

		private static IEnumerable<BlockFlowTestCase> getLinePrefixFlowTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var oneHundredSpacesAndTabs = String.Join(String.Empty, Enumerable.Repeat("\t ", 50));
			
			foreach (var type in BlockFlowCache.GetFlowTypes())
			{
				yield return new BlockFlowTestCase(
					type, 
					value: "\tABC\t  ",
					wholeCapture: "\t",
					firstParenthesisCapture: "\t"
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + "\tABC\t  ",
					wholeCapture: oneHundredSpaces + "\t",
					firstParenthesisCapture: "\t"
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + " ABC\t  ",
					wholeCapture: oneHundredSpaces + " ",
					firstParenthesisCapture: " "
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + " \tABC\t  ",
					wholeCapture: oneHundredSpaces + " \t",
					firstParenthesisCapture: " \t"
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + "\t ABC\t  ",
					wholeCapture: oneHundredSpaces + "\t ",
					firstParenthesisCapture: "\t "
				);
				yield return new BlockFlowTestCase(
					type, 
					value: oneHundredSpaces + oneHundredSpacesAndTabs + "\t ABC\t  ",
					wholeCapture: oneHundredSpaces + oneHundredSpacesAndTabs,
					firstParenthesisCapture: oneHundredSpacesAndTabs
				);
			}
		}

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _linePrefixBlockFlowRegexByType =
			BlockFlowCache.GetBlockAndFlowTypes().ToDictionary(
				i => i,
				i => new Regex(GlobalConstants.LinePrefix(i), RegexOptions.Compiled)
			);
	}
}