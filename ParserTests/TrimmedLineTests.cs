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
		public void TrimmedLine_ReturnsCorrespondingRegexForBlockFlow(BlockFlowInOut value, string expectedRegex)
		{
			var actualRegex = GlobalConstants.TrimmedLine(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		[TestCaseSource(nameof(getTrimmedLineBlockTestCases))]
		public void TrimmedLine_Blocks_LineBreakAndEmptyLine_Matches(BlockFlowTestCase testCase)
		{
			var regex = _trimmedLineRegex[testCase.Type];

			var match = regex.Matches(testCase.Value);

			Assert.That(match.Count, Is.EqualTo(1));

			Assert.Multiple(
				() =>
				{
					Assert.That(match[0].Groups.Count, Is.EqualTo(2));
					Assert.That(match[0].Groups[0].Captures.Count, Is.EqualTo(1));
					Assert.That(match[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
					Assert.That(
						match[0].Groups[1].Captures.Select(c => c.Value),
						Is.All.EqualTo(testCase.FirstParenthesisCapture)
					);
				}
			);
		}

		[TestCaseSource(nameof(getTrimmedLineFlowTestCases))]
		public void TrimmedLine_Flows_LineBreakAndEmptyLine_Matches(BlockFlowTestCase testCase)
		{
			var regex = _trimmedLineRegex[testCase.Type];

			var match = regex.Matches(testCase.Value);

			Assert.That(match.Count, Is.EqualTo(1));

			Assert.Multiple(
				() =>
				{
					Assert.That(match[0].Groups.Count, Is.EqualTo(3));
					Assert.That(match[0].Groups[0].Captures.Count, Is.EqualTo(1));
					Assert.That(match[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
					Assert.That(
						match[0].Groups[1].Captures.Select(c => c.Value),
						Is.All.EqualTo(testCase.FirstParenthesisCapture)
					);
					if (testCase.SecondParenthesisCapture is null)
					{
						Assert.That(match[0].Groups[2].Value, Is.Empty);
					}
					else
					{
						Assert.That(
							match[0].Groups[2].Captures.Select(c => c.Value),
							Is.All.EqualTo(testCase.SecondParenthesisCapture)
						);
					}
				}
			);
		}

		[Test]
		public void TrimmedLine_NoEmptyLines_DoesNotMatch(
			[ValueSource(nameof(GetBlocksAndFlows))] BlockFlowInOut type,
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
			foreach (var value in BlockFlowCache.GetBlockAndFlowTypes())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(value, $"{newLine}" + "( {0,100}" + newLine + ")+");
						break;
					case BlockFlowInOut.FlowOut:
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(value, $"{newLine}" + "( {0,100}([ \t]{1,100})?" + newLine + ")+");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineBlockTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var newLine = Environment.NewLine;
			var emptyLine = oneHundredSpaces + newLine;

			foreach (var type in BlockFlowCache.GetBlockTypes())
			{
				yield return new BlockFlowTestCase(
					type,
					value: newLine + newLine + "\tABC\t  ",
					wholeCapture: newLine + newLine,
					firstParenthesisCapture: newLine
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + newLine + "\tABC\t  ",
					wholeCapture: newLine + newLine,
					firstParenthesisCapture: newLine
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + emptyLine + "\tABC\t  ",
					wholeCapture: newLine + emptyLine,
					firstParenthesisCapture: emptyLine
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + emptyLine + emptyLine + "\tABC\t  ",
					wholeCapture: newLine + emptyLine + emptyLine,
					firstParenthesisCapture: emptyLine
				);
			}
		}

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineFlowTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var oneHundredSpacesAndTabs = String.Join(String.Empty, Enumerable.Repeat("\t ", 50));
			var newLine = Environment.NewLine;

			var emptyLineWithSpacesOnly = oneHundredSpaces + newLine;
			var emptyLineWithSpacesAndTabs = oneHundredSpaces + oneHundredSpacesAndTabs + newLine;

			foreach (var type in BlockFlowCache.GetFlowTypes())
			{
				yield return new BlockFlowTestCase(
					type,
					value: newLine + newLine + "\tABC\t  ",
					wholeCapture: newLine + newLine,
					firstParenthesisCapture: newLine
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + newLine + "\tABC\t  ",
					wholeCapture: newLine + newLine,
					firstParenthesisCapture: newLine
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + emptyLineWithSpacesOnly + "\tABC\t  ",
					wholeCapture: newLine + emptyLineWithSpacesOnly,
					firstParenthesisCapture: emptyLineWithSpacesOnly
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + emptyLineWithSpacesOnly + emptyLineWithSpacesOnly + "\tABC\t  ",
					wholeCapture: newLine + emptyLineWithSpacesOnly + emptyLineWithSpacesOnly,
					firstParenthesisCapture: emptyLineWithSpacesOnly
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + oneHundredSpacesAndTabs + newLine + "\tABC\t  ",
					wholeCapture: newLine + oneHundredSpacesAndTabs + newLine,
					firstParenthesisCapture: oneHundredSpacesAndTabs + newLine,
					secondParenthesisCapture: oneHundredSpacesAndTabs
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + emptyLineWithSpacesAndTabs + "\tABC\t  ",
					wholeCapture: newLine + emptyLineWithSpacesAndTabs,
					firstParenthesisCapture: emptyLineWithSpacesAndTabs,
					secondParenthesisCapture: oneHundredSpacesAndTabs
				);
				yield return new BlockFlowTestCase(
					type,
					value: "\tABC\t  " + newLine + emptyLineWithSpacesAndTabs + emptyLineWithSpacesAndTabs + "\tABC\t  ",
					wholeCapture: newLine + emptyLineWithSpacesAndTabs + emptyLineWithSpacesAndTabs,
					firstParenthesisCapture: emptyLineWithSpacesAndTabs,
					secondParenthesisCapture: oneHundredSpacesAndTabs
				);
			}
		}

		private static IEnumerable<string> getTrimmedLineNonMatchableCases()
		{
			var newLine = Environment.NewLine;
			yield return $"ABC  {newLine}  ABC";
			yield return $"ABC\t{newLine}\tABC";
		}

		private static IEnumerable<BlockFlowInOut> GetBlocksAndFlows()
		{
			return BlockFlowCache.GetBlockAndFlowTypes();
		}

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _trimmedLineRegex =
			BlockFlowCache.GetBlockAndFlowTypes().ToDictionary(
				i => i,
				i => new Regex(GlobalConstants.TrimmedLine(i), RegexOptions.Compiled)
			);
	}
}