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

		private static IEnumerable<TestCaseData> getTrimmedLineBlockFlowWithCorrespondingRegex()
		{
			var emptyLinePostfix = "(\r\n?|\n)";
			foreach (var value in BlockFlowCache.GetBlocksAndFlows())
			{
				switch (value)
				{
					case BlockFlowInOut.BlockOut:
					case BlockFlowInOut.BlockIn:
						yield return new TestCaseData(
							value,
							$"{emptyLinePostfix}" + "(^ {1,100}" + emptyLinePostfix + ")+"
						);
						break;
					case BlockFlowInOut.FlowOut:
					case BlockFlowInOut.FlowIn:
						yield return new TestCaseData(
							value,
							$"{emptyLinePostfix}" + "(^ {1,100}([ \t]{1,100})?" + emptyLinePostfix + ")+"
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