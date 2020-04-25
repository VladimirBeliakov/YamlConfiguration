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

		private static IEnumerable<BlockFlowTestCase> getTrimmedLineBlockTestCases()
		{
			var oneHundredSpaces = new String(Enumerable.Repeat(' ', 100).ToArray());
			var lineBreaks = new[] { "\r\n", "\r", "\n" };

			foreach (var lineBreak in lineBreaks)
			{
				foreach (var type in BlockFlowCache.GetBlockTypes())
				{
					yield return new BlockFlowTestCase(
						type, 
						value: lineBreak + "\tABC\t  ",
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

		private static IEnumerable<TestCaseData> getTrimmedLineBlockFlowWithCorrespondingRegex()
		{
			var emptyLine = "(\r\n?|\n)";
			foreach (var value in BlockFlowCache.GetBlocksAndFlows())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(
							value,
							$"{emptyLine}" + "(^ {0,100}" + emptyLine + ")+"
						);
						break;
					case BlockFlowInOut.FlowOut:
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(
							value,
							$"{emptyLine}" + "(^ {0,100}([ \t]{1,100})?" + emptyLine + ")+"
						);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IReadOnlyDictionary<BlockFlowInOut, Regex> _trimmedLineRegex = BlockFlowCache.GetBlocksAndFlows()
			.ToDictionary(
				i => i,
				i => new Regex(GlobalConstants.TrimmedLine(i), RegexOptions.Compiled | RegexOptions.Multiline)
			);
	}
}