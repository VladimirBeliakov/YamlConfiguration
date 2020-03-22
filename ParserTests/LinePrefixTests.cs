using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Parser;
using Parser.TypeDefinitions;

namespace DeserializerTests
{
	[TestFixture, Parallelizable(ParallelScope.Children)]
	public class LinePrefixTests
	{
		[TestCaseSource(nameof(getBlockFlowWithCorrespondingRegex))]
		public void LinePrefix_ReturnsCorrespondingRegexForBlockFlow(BlockFlowInOut value, string expectedRegex)
		{
			var actualRegex = GlobalConstants.LinePrefix(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		[TestCaseSource(nameof(getBlockTestCases))]
		public void LinePrefix_Blocks_SpacesAtBeginning_Matches(BlockFlowTestCase testCase)
		{
			var regex = _blockFlowRegexByType[testCase.Type];

			var matches = regex.Matches(testCase.Value);

			Assert.Multiple(() =>
			{
				Assert.That(matches.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
			});
		}

		[TestCaseSource(nameof(getFlowTestCases))]
		public void LinePrefix_Flows_SpacesAndTabsAtBeginning_Matches(BlockFlowTestCase testCase)
		{
			var regex = _blockFlowRegexByType[testCase.Type];

			var matches = regex.Matches(testCase.Value);

			Assert.Multiple(() =>
			{
				Assert.That(matches.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups.Count, Is.EqualTo(2));
				Assert.That(matches[0].Groups[0].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
				Assert.That(matches[0].Groups[1].Captures.Count, Is.EqualTo(1));
				Assert.That(matches[0].Groups[1].Captures[0].Value, Is.EqualTo(testCase.ParenthesisCapture));
			});
		}

		[Test]
		public void LinePrefix_NoSpacesAtBeginning_DoesNotMatch(
			[ValueSource(nameof(getBlocksAndFlows))] BlockFlowInOut type,
			[Values("ABC   ", "\tABC   ")] string testValue
		)
		{
			var regex = _blockFlowRegexByType[type];

			var matches = regex.Matches(testValue);

			Assert.That(matches.Count, Is.EqualTo(0));
		}
		
		private static IEnumerable<TestCaseData> getBlockFlowWithCorrespondingRegex()
		{
			foreach (var value in getBlocksAndFlows())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
						yield return new TestCaseData(value, "^ {1,100}");
						break;
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(value, "^ {1,100}");
						break;
					case BlockFlowInOut.FlowOut:
						yield return new TestCaseData(value, "^ {1,100}([ \t]{1,100})?");
						break;
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(value, "^ {1,100}([ \t]{1,100})?");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowInOut> getBlocksAndFlows()
		{
			return Enum.GetValues(typeof(BlockFlowInOut)).Cast<BlockFlowInOut>();
		}

		private static IEnumerable<BlockFlowInOut> getBlocks()
		{
			return getBlocksAndFlows().Except(getFlows());
		}

		private static IEnumerable<BlockFlowInOut> getFlows()
		{
			yield return BlockFlowInOut.FlowIn;
			yield return BlockFlowInOut.FlowOut;
		}

		private static IEnumerable<BlockFlowTestCase> getBlockTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			
			foreach (var value in getBlocks())
			{
				yield return new BlockFlowTestCase(
					value, 
					value: oneHundredSpaces + "\tABC\t  ",
					wholeCapture: oneHundredSpaces
				);
				yield return new BlockFlowTestCase(
					value, 
					value: oneHundredSpaces + " ABC\t  ",
					wholeCapture: oneHundredSpaces
				);
			}
		}
		
		private static IEnumerable<BlockFlowTestCase> getFlowTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			
			foreach (var value in getFlows())
			{
				yield return new BlockFlowTestCase(
					value, 
					value: oneHundredSpaces + "\tABC\t  ",
					wholeCapture: oneHundredSpaces + "\t",
					parenthesisCapture: "\t"
				);
				yield return new BlockFlowTestCase(
					value, 
					value: oneHundredSpaces + " ABC\t  ",
					wholeCapture: oneHundredSpaces + " ",
					parenthesisCapture: " "
				);
				yield return new BlockFlowTestCase(
					value, 
					value: oneHundredSpaces + " \tABC\t  ",
					wholeCapture: oneHundredSpaces + " \t",
					parenthesisCapture: " \t"
				);
				yield return new BlockFlowTestCase(
					value, 
					value: oneHundredSpaces + "\t ABC\t  ",
					wholeCapture: oneHundredSpaces + "\t ",
					parenthesisCapture: "\t "
				);
			}
		}

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _blockFlowRegexByType =
			getBlocksAndFlows()
				.ToDictionary(i => i, i => new Regex(GlobalConstants.LinePrefix(i), RegexOptions.Compiled));
	}
}