using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class LinePrefixTests
	{
		[TestCaseSource(nameof(getBlockFlowWithCorrespondingRegex))]
		public void LinePrefix_ReturnsCorrespondingRegexForBlockFlow(Context value, RegexPattern expectedRegex)
		{
			var actualRegex = BasicStructures.LinePrefix(value);

			Assert.That(actualRegex, Is.EqualTo(expectedRegex));
		}

		[TestCaseSource(nameof(getBlockTestCases))]
		public void LinePrefix_Blocks_SpacesAtBeginning_Matches(BlockFlowTestCase testCase)
		{
			var regex = _linePrefixBlockFlowRegexByType[testCase.Type];

			var match = regex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		[TestCaseSource(nameof(getFlowTestCases))]
		public void LinePrefix_Flows_SpacesAndTabsAtBeginning_Matches(BlockFlowTestCase testCase)
		{
			var regex = _linePrefixBlockFlowRegexByType[testCase.Type];

			var match = regex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeCapture));
		}

		// TODO: Think about how negative cases could be tested.

		private static IEnumerable<TestCaseData> getBlockFlowWithCorrespondingRegex()
		{
			foreach (var value in EnumCache.GetBlockAndFlowTypes())
			{
				switch (value)
				{
					case Context.BlockOut:
					case Context.BlockIn:
						yield return new TestCaseData(value, (RegexPattern) "^(?: ){0,1000}");
						break;
					case Context.FlowOut:
					case Context.FlowIn:
						yield return new TestCaseData(value, (RegexPattern) "^(?: ){0,1000}(?:(?:^|[ \t]{1,1000}))?");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static IEnumerable<BlockFlowTestCase> getCommonTestCases(Context type)
		{
			var spaces = CharStore.Spaces;

			yield return new BlockFlowTestCase(
				type, 
				testValue: String.Empty + "ABC",
				wholeCapture: String.Empty
			);
			yield return new BlockFlowTestCase(
				type, 
				testValue: spaces + "ABC",
				wholeCapture: spaces
			);
		}

		private static IEnumerable<BlockFlowTestCase> getBlockTestCases()
		{
			return EnumCache.GetBlockTypes().SelectMany(getCommonTestCases);
		}

		private static IEnumerable<BlockFlowTestCase> getFlowTestCases()
		{
			var spaces = CharStore.Spaces;
			var spacesAndTabs = CharStore.SpacesAndTabs;
			
			foreach (var type in EnumCache.GetFlowTypes())
			{
				foreach (var testCase in getCommonTestCases(type))
					yield return testCase;

				yield return new BlockFlowTestCase(
					type, 
					testValue: spaces + spacesAndTabs + "ABC",
					wholeCapture: spaces + spacesAndTabs
				);
			}
		}

		private static readonly IReadOnlyDictionary<Context, Regex> _linePrefixBlockFlowRegexByType =
			EnumCache.GetBlockAndFlowTypes().ToDictionary(
				i => i,
				i => new Regex(BasicStructures.LinePrefix(i), RegexOptions.Compiled)
			);
	}
}