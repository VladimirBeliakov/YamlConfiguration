using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using Processor;

namespace ProcessorTests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class NodeTagsTests
	{
		[TestCaseSource(nameof(getTagAnchorTestCases))]
		public void TagAnchor_ValidNodeTags_Matches(RegexTestCase testCase)
		{
			var match = _tagAnchorPropertiesRegex.Match(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
					Assert.That(match.Groups.Count, Is.EqualTo(3));
					Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.Captures[0]));
					if (testCase.Captures[1] != null)
					{
						Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(1));
						Assert.That(match.Groups[2].Captures[0].Value, Is.EqualTo(testCase.Captures[1]));
					}
				}
			);
		}

		[TestCaseSource(nameof(getAnchorTagTestCases))]
		public void AnchorTag_ValidNodeTags_Matches(RegexTestCase testCase)
		{
			var match = _anchorTagPropertiesRegex.Match(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
					Assert.That(match.Groups.Count, Is.EqualTo(3));
					Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.Captures[0]));
					if (testCase.Captures[1] != null)
					{
						Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(1));
						Assert.That(match.Groups[2].Captures[0].Value, Is.EqualTo(testCase.Captures[1]));
					}
				}
			);
		}

		private static IEnumerable<RegexTestCase> getTagAnchorTestCases()
		{
			var spaceWithChars = " " + CharCache.Chars;

			var (tagProperties, separateInLines, anchorNames, anyTagProperty, anySeparateInLine, anyAnchorName) =
				_commonCases.Value;

			foreach (var tagProperty in tagProperties)
			{
				yield return new RegexTestCase(
					testValue: tagProperty + anySeparateInLine + "&" + anyAnchorName + spaceWithChars,
					wholeMatch: tagProperty + anySeparateInLine + "&" + anyAnchorName,
					tagProperty,
					anyAnchorName
				);
			}

			foreach (var separateInLine in separateInLines)
			{
				yield return new RegexTestCase(
					testValue: anyTagProperty + separateInLine + "&" + anyAnchorName + spaceWithChars,
					wholeMatch: anyTagProperty + separateInLine + "&" + anyAnchorName,
					anyTagProperty,
					anyAnchorName
				);
			}

			foreach (var anchorName in anchorNames)
			{
				yield return new RegexTestCase(
					testValue: anyTagProperty + anySeparateInLine + "&" + anchorName + spaceWithChars,
					wholeMatch: anyTagProperty + anySeparateInLine + "&" + anchorName,
					anyTagProperty,
					anchorName
				);
			}

			yield return new RegexTestCase(
				testValue: anyTagProperty + anySeparateInLine + spaceWithChars,
				wholeMatch: anyTagProperty,
				anyTagProperty,
				null
			);
		}

		private static IEnumerable<RegexTestCase> getAnchorTagTestCases()
		{
			var spaceWithChars = " " + CharCache.Chars;

			var (tagProperties, separateInLines, anchorNames, anyTagProperty, anySeparateInLine, anyAnchorName) =
				_commonCases.Value;

			foreach (var anchorName in anchorNames)
			{
				yield return new RegexTestCase(
					testValue: "&" + anchorName + anySeparateInLine + anyTagProperty + spaceWithChars,
					wholeMatch: "&" + anchorName + anySeparateInLine + anyTagProperty,
					anchorName,
					anyTagProperty
				);
			}

			foreach (var separateInLine in separateInLines)
			{
				yield return new RegexTestCase(
					testValue: "&" + anyAnchorName + separateInLine + anyTagProperty + spaceWithChars,
					wholeMatch: "&" + anyAnchorName + separateInLine + anyTagProperty,
					anyAnchorName,
					anyTagProperty
				);
			}

			foreach (var tagProperty in tagProperties)
			{
				yield return new RegexTestCase(
					testValue: "&" + anyAnchorName + anySeparateInLine + tagProperty + spaceWithChars,
					wholeMatch: "&" + anyAnchorName + anySeparateInLine + tagProperty,
					anyAnchorName,
					tagProperty
				);
			}

			yield return new RegexTestCase(
				testValue: "&" + anyAnchorName + anySeparateInLine + spaceWithChars,
				wholeMatch: "&" + anyAnchorName,
				anyAnchorName,
				null
			);
		}

		private static IEnumerable<string> getShorthandTags()
		{
			var tagHandles = CharCache.GetTagHandles().ToList();
			var tagChars = CharCache.GetTagChars().ToList();

			var anyTagHandle = tagHandles.First();
			var anyTagChar = tagChars.First();

			foreach (var tagHandle in tagHandles)
				yield return tagHandle + anyTagChar;

			foreach (var tagChar in tagChars)
				yield return anyTagHandle + tagChar;
		}

		private static (
			IReadOnlyCollection<string> tagProperties,
			IReadOnlyCollection<string> separateInLines,
			IReadOnlyCollection<string> anchorNames,
			string anyTagProperty,
			string anySeparateInLine,
			string anyAnchorName
		) getCommonCases()
		{
			var verbatimTags = CharCache.GetUriCharGroups().Select(g => $"!<{g}>");
			var shorthandTags = getShorthandTags();
			const string nonSpecificTag = "!";

			var tagProperties = verbatimTags.Concat(shorthandTags).Append(nonSpecificTag).ToList();
			var separateInLines = CharCache.SeparateInLineCases;
			var anchorNames = CharCache.GetAnchorCharGroups().ToList();

			var anyTagProperty = tagProperties.First();
			var anySeparateInLine = separateInLines.First();
			var anyAnchorName = anchorNames.First();

			return (tagProperties, separateInLines, anchorNames, anyTagProperty, anySeparateInLine, anyAnchorName);
		}

		private static readonly
			Lazy<(
				IReadOnlyCollection<string> tagProperties,
				IReadOnlyCollection<string> separateInLines,
				IReadOnlyCollection<string> anchorNames,
				string anyTagProperty,
				string anySeparateInLine,
				string anyAnchorName
			)> _commonCases =
				new Lazy<(
					IReadOnlyCollection<string> tagProperties,
					IReadOnlyCollection<string> separateInLines,
					IReadOnlyCollection<string> anchorNames,
					string anyTagProperty,
					string anySeparateInLine,
					string anyAnchorName
				)>(getCommonCases, LazyThreadSafetyMode.ExecutionAndPublication);

		private static readonly Regex _tagAnchorPropertiesRegex = new Regex(
			BasicStructures.NodeTags.TagAnchorProperties,
			RegexOptions.Compiled
		);

		private static readonly Regex _anchorTagPropertiesRegex = new Regex(
			BasicStructures.NodeTags.AnchorTagProperties,
			RegexOptions.Compiled
		);
	}
}