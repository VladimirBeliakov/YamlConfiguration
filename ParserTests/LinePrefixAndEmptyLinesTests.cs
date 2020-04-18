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
	public class LinePrefixAndEmptyLinesTests
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

		[Test]
		public void LinePrefix_NoSpacesAtBeginning_DoesNotMatch(
			[ValueSource(nameof(getBlocksAndFlows))] BlockFlowInOut type,
			[Values("ABC   ", "\tABC   ")] string testValue
		)
		{
			var regex = _linePrefixBlockFlowRegexByType[type];

			var matches = regex.Matches(testValue);

			Assert.That(matches.Count, Is.EqualTo(0));
		}

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
			[ValueSource(nameof(getBlocksAndFlows))] BlockFlowInOut type,
			[Values("ABC   ", "\tABC   ")] string testValue
		)
		{
			var regex = _emptyLineBlockFlowRegexByType[type];

			var matches = regex.Matches(testValue);

			Assert.That(matches.Count, Is.EqualTo(0));
		}

		private static IEnumerable<TestCaseData> getLinePrefixBlockFlowWithCorrespondingRegex()
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

		// TODO: Continue from here
		private static IEnumerable<TestCaseData> getEmptyLineBlockFlowWithCorrespondingRegex()
		{
			var emptyLinePostfix = "(\r\n?|\n)";
			foreach (var value in getBlocksAndFlows())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
						yield return new TestCaseData(value, "^ {1,100}" + emptyLinePostfix);
						break;
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(value, "^ {1,100}" + emptyLinePostfix);
						break;
					case BlockFlowInOut.FlowOut:
						yield return new TestCaseData(value, "^ {1,100}([ \t]{1,100})?" + emptyLinePostfix);
						break;
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(value, "^ {1,100}([ \t]{1,100})?" + emptyLinePostfix);
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

		private static IEnumerable<BlockFlowInOut> getBlockTypes()
		{
			return getBlocksAndFlows().Except(getFlowTypes());
		}

		private static IEnumerable<BlockFlowInOut> getFlowTypes()
		{
			yield return BlockFlowInOut.FlowIn;
			yield return BlockFlowInOut.FlowOut;
		}

		private static IEnumerable<BlockFlowTestCase> getLinePrefixBlockTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());

			foreach (var type in getBlockTypes())
			{
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
			var oneHundredSpacesAndTabs =
				new String(Enumerable.Repeat('\t', 50).Concat(Enumerable.Repeat(' ', 50)).ToArray());
			
			foreach (var type in getFlowTypes())
			{
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

		private static IEnumerable<BlockFlowTestCase> getEmptyLineBlockTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var lineBreaks = new[] { "\r\n", "\r", "\n" };

			foreach (var lineBreak in lineBreaks)
			{
				foreach (var type in getBlockTypes())
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
				foreach (var type in getFlowTypes())
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

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _linePrefixBlockFlowRegexByType =
			getBlocksAndFlows()
				.ToDictionary(i => i, i => new Regex(GlobalConstants.LinePrefix(i), RegexOptions.Compiled));

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _emptyLineBlockFlowRegexByType =
			getBlocksAndFlows()
				.ToDictionary(i => i, i => new Regex(GlobalConstants.EmptyLine(i), RegexOptions.Compiled));
	}
}