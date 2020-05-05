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
					Assert.That(match[0].Groups.Count, Is.EqualTo(4));
					Assert.That(match[0].Groups[0].Captures.Count, Is.EqualTo(1));
					Assert.That(match[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
					Assert.That(
						match[0].Groups[1].Captures.Select(c => c.Value),
						Is.All.EqualTo(testCase.FirstParenthesisCapture)
					);
					Assert.That(
						match[0].Groups[2].Captures.Select(c => c.Value),
						Is.All.EqualTo(testCase.SecondParenthesisCapture)
					);
					Assert.That(
						match[0].Groups[3].Captures.Select(c => c.Value),
						Is.All.EqualTo(testCase.ThirdParenthesisCapture)
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
					Assert.That(match[0].Groups.Count, Is.EqualTo(5));
					Assert.That(match[0].Groups[0].Captures.Count, Is.EqualTo(1));
					Assert.That(match[0].Groups[0].Captures[0].Value, Is.EqualTo(testCase.WholeCapture));
					Assert.That(
						match[0].Groups[1].Captures.Select(c => c.Value),
						Is.All.EqualTo(testCase.FirstParenthesisCapture)
					);
					Assert.That(
						match[0].Groups[2].Captures.Select(c => c.Value),
						Is.All.EqualTo(testCase.SecondParenthesisCapture)
					);
					if (testCase.ThirdParenthesisCapture is null)
					{
						Assert.That(match[0].Groups[3].Value, Is.Empty);
					}
					else
					{
						Assert.That(
							match[0].Groups[3].Captures.Select(c => c.Value),
							Is.All.EqualTo(testCase.ThirdParenthesisCapture)
						);
					}
					Assert.That(
						match[0].Groups[4].Captures.Select(c => c.Value),
						Is.All.EqualTo(testCase.ForthParenthesisCapture)
					);
				}
			);
		}

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineBlockTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var newLines = new[] { "\r\n", "\r", "\n" };

			foreach (var newLine in newLines)
			{
				var emptyLine = oneHundredSpaces + newLine;

				foreach (var type in BlockFlowCache.GetBlockTypes())
				{
					yield return new BlockFlowTestCase(
						type,
						value: newLine + newLine + "\tABC\t  ",
						wholeCapture: newLine + newLine,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: newLine,
						thirdParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + newLine + "\tABC\t  ",
						wholeCapture: newLine + newLine,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: newLine,
						thirdParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + emptyLine + "\tABC\t  ",
						wholeCapture: newLine + emptyLine,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: emptyLine,
						thirdParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + emptyLine + emptyLine + "\tABC\t  ",
						wholeCapture: newLine + emptyLine + emptyLine,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: emptyLine,
						thirdParenthesisCapture: newLine
					);
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineFlowTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var oneHundredSpacesAndTabs = String.Join(String.Empty, Enumerable.Repeat("\t ", 50));
			var newLines = new[] { "\r\n", "\r", "\n" };

			foreach (var newLine in newLines)
			{
				var emptyLineWithSpaces = oneHundredSpaces + newLine;
				var emptyLineWithSpacesAndTabs = oneHundredSpaces + oneHundredSpacesAndTabs + newLine;

				foreach (var type in BlockFlowCache.GetFlowTypes())
				{
					yield return new BlockFlowTestCase(
						type,
						value: newLine + newLine + "\tABC\t  ",
						wholeCapture: newLine + newLine,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: newLine,
						forthParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + newLine + "\tABC\t  ",
						wholeCapture: newLine + newLine,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: newLine,
						forthParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + emptyLineWithSpaces + "\tABC\t  ",
						wholeCapture: newLine + emptyLineWithSpaces,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: emptyLineWithSpaces,
						forthParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + emptyLineWithSpaces + emptyLineWithSpaces + "\tABC\t  ",
						wholeCapture: newLine + emptyLineWithSpaces + emptyLineWithSpaces,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: emptyLineWithSpaces,
						forthParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + oneHundredSpacesAndTabs + newLine + "\tABC\t  ",
						wholeCapture: newLine + oneHundredSpacesAndTabs + newLine,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: oneHundredSpacesAndTabs + newLine,
						thirdParenthesisCapture: oneHundredSpacesAndTabs,
						forthParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + emptyLineWithSpacesAndTabs + "\tABC\t  ",
						wholeCapture: newLine + emptyLineWithSpacesAndTabs,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: emptyLineWithSpacesAndTabs,
						thirdParenthesisCapture: oneHundredSpacesAndTabs,
						forthParenthesisCapture: newLine
					);
					yield return new BlockFlowTestCase(
						type,
						value: "\tABC\t  " + newLine + emptyLineWithSpacesAndTabs + emptyLineWithSpacesAndTabs + "\tABC\t  ",
						wholeCapture: newLine + emptyLineWithSpacesAndTabs + emptyLineWithSpacesAndTabs,
						firstParenthesisCapture: newLine,
						secondParenthesisCapture: emptyLineWithSpacesAndTabs,
						thirdParenthesisCapture: oneHundredSpacesAndTabs,
						forthParenthesisCapture: newLine
					);
				}
			}
		}

		private static IEnumerable<TestCaseData> getTrimmedLineBlockFlowWithCorrespondingRegex()
		{
			var newLine = "(\r\n?|\n)";
			foreach (var value in BlockFlowCache.GetBlockAndFlowTypes())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(
							value,
							$"{newLine}" + "( {0,100}" + newLine + ")+"
						);
						break;
					case BlockFlowInOut.FlowOut:
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(
							value,
							$"{newLine}" + "( {0,100}([ \t]{1,100})?" + newLine + ")+"
						);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static readonly IReadOnlyDictionary<BlockFlowInOut, Regex> _trimmedLineRegex =
			BlockFlowCache.GetBlockAndFlowTypes().ToDictionary(
				i => i,
				i => new Regex(GlobalConstants.TrimmedLine(i), RegexOptions.Compiled)
			);
	}
}