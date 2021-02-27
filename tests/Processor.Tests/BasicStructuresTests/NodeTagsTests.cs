using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class NodeTagsTests
	{
		[TestCaseSource(nameof(getTagAnchorPositiveTestCases))]
		public void TagAnchor_ValidNodeTags_Matches(RegexTestCase testCase)
		{
			var match = _tagAnchorPropertiesRegex.Match(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
					Assert.That(match.Groups.Count, Is.EqualTo(3));
					Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.Captures?.FirstOrDefault()));
					if (testCase.Captures?.ElementAtOrDefault(1) != null)
					{
						Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(1));
						Assert.That(match.Groups[2].Captures[0].Value, Is.EqualTo(testCase.Captures[1]));
					}
				}
			);
		}

		[TestCaseSource(nameof(getAnchorTagPositiveTestCases))]
		public void AnchorTag_ValidNodeTags_Matches(RegexTestCase testCase)
		{
			var match = _anchorTagPropertiesRegex.Match(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
					Assert.That(match.Groups.Count, Is.EqualTo(3));
					Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.Captures?.FirstOrDefault()));
					if (testCase.Captures?.ElementAtOrDefault(1) != null)
					{
						Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(1));
						Assert.That(match.Groups[2].Captures[0].Value, Is.EqualTo(testCase.Captures[1]));
					}
				}
			);
		}

		[TestCaseSource(nameof(getTagAnchorNegativeTestCases))]
		public void TagAnchor_InvalidProperty_DoesNotMatch(string testCase)
		{
			var match = _tagAnchorPropertiesRegex.Match(testCase);

			Assert.False(match.Success);
		}

		[TestCaseSource(nameof(getAnchorTagNegativeTestCases))]
		public void AnchorTag_InvalidProperty_DoesNotMatch(string testCase)
		{
			var match = _anchorTagPropertiesRegex.Match(testCase);

			Assert.False(match.Success);
		}

		private static IEnumerable<RegexTestCase> getTagAnchorPositiveTestCases()
		{
			var chars = CharStore.Chars;

			var (tagProperties, separateInLines, anchorNames, anyTagProperty, anySeparateInLine, anyAnchorName) =
				_commonCases.Value;

			foreach (var tagProperty in tagProperties)
			{
				yield return new RegexTestCase(
					testValue: anySeparateInLine + tagProperty + anySeparateInLine + "&" + anyAnchorName +
							   anySeparateInLine + chars,
					wholeMatch: anySeparateInLine + tagProperty + anySeparateInLine + "&" + anyAnchorName +
								anySeparateInLine,
					tagProperty,
					anyAnchorName
				);
			}

			foreach (var separateInLine in separateInLines)
			{
				yield return new RegexTestCase(
					testValue: separateInLine + anyTagProperty + separateInLine + "&" + anyAnchorName + separateInLine +
							   chars,
					wholeMatch: separateInLine + anyTagProperty + separateInLine + "&" + anyAnchorName + separateInLine,
					anyTagProperty,
					anyAnchorName
				);
			}

			foreach (var anchorName in anchorNames)
			{
				yield return new RegexTestCase(
					testValue: anySeparateInLine + anyTagProperty + anySeparateInLine + "&" + anchorName +
							   anySeparateInLine + chars,
					wholeMatch: anySeparateInLine + anyTagProperty + anySeparateInLine + "&" + anchorName +
								anySeparateInLine,
					anyTagProperty,
					anchorName
				);
			}

			yield return new RegexTestCase(
				testValue: anySeparateInLine + anyTagProperty + anySeparateInLine + chars,
				wholeMatch: anySeparateInLine + anyTagProperty + anySeparateInLine,
				anyTagProperty,
				null!
			);

			yield return new RegexTestCase(
				testValue: anyTagProperty + anySeparateInLine + chars,
				wholeMatch: anyTagProperty + anySeparateInLine ,
				anyTagProperty,
				null!
			);
		}

		private static IEnumerable<RegexTestCase> getAnchorTagPositiveTestCases()
		{
			var chars = CharStore.Chars;

			var (tagProperties, separateInLines, anchorNames, anyTagProperty, anySeparateInLine, anyAnchorName) =
				_commonCases.Value;

			foreach (var anchorName in anchorNames)
			{
				yield return new RegexTestCase(
					testValue: anySeparateInLine + "&" + anchorName + anySeparateInLine + anyTagProperty +
							   anySeparateInLine + chars,
					wholeMatch: anySeparateInLine + "&" + anchorName + anySeparateInLine + anyTagProperty +
								anySeparateInLine,
					anchorName,
					anyTagProperty
				);
			}

			foreach (var separateInLine in separateInLines)
			{
				yield return new RegexTestCase(
					testValue: separateInLine + "&" + anyAnchorName + separateInLine + anyTagProperty + separateInLine +
							   chars,
					wholeMatch: separateInLine + "&" + anyAnchorName + separateInLine + anyTagProperty + separateInLine,
					anyAnchorName,
					anyTagProperty
				);
			}

			foreach (var tagProperty in tagProperties)
			{
				yield return new RegexTestCase(
					testValue: anySeparateInLine + "&" + anyAnchorName + anySeparateInLine + tagProperty +
							   anySeparateInLine + chars,
					wholeMatch: anySeparateInLine + "&" + anyAnchorName + anySeparateInLine + tagProperty +
								anySeparateInLine,
					anyAnchorName,
					tagProperty
				);
			}

			yield return new RegexTestCase(
				testValue: anySeparateInLine + "&" + anyAnchorName + anySeparateInLine + chars,
				wholeMatch: anySeparateInLine + "&" + anyAnchorName + anySeparateInLine,
				anyAnchorName,
				null!
			);

			yield return new RegexTestCase(
				testValue: "&" + anyAnchorName + anySeparateInLine + chars,
				wholeMatch: "&" + anyAnchorName + anySeparateInLine,
				anyAnchorName,
				null!
			);
		}

		private static IEnumerable<string> getShorthandTags()
		{
			var tagHandles = CharStore.GetTagHandles().ToList();
			var tagChars = CharStore.GetTagChars().ToList();

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
			var verbatimTags = CharStore.GetUriCharGroups().Select(g => $"!<{g}>");
			var shorthandTags = getShorthandTags();
			const string nonSpecificTag = "!";

			var tagProperties = verbatimTags.Concat(shorthandTags).Append(nonSpecificTag).ToList();
			var separateInLines = CharStore.SeparateInLineCases;
			var anchorNames = CharStore.GetNsCharGroups(CharStore.FlowIndicators).ToList();

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

		private static IEnumerable<string> getTagAnchorNegativeTestCases()
		{
			// Verbatim tag
			yield return "!<%0f>&a ";
			yield return "<%0f> &a ";
			yield return "!%0f> &a ";
			yield return "!<%0f &a ";
			yield return "!<%> &a ";
			yield return "!<%f> &a ";
			yield return "!< %0f> &a ";
			yield return "!<%0f > &a ";
			yield return $"!<{CharStore.GetCharRange("0") + "0"}> &a ";

			// Shorthand tag
			yield return "!0z! &a ";
			yield return "!0z!0z&a";
			yield return "0z!0z &a ";
			yield return "!0z!0z% &a ";
			yield return "!0z%!0z &a ";
			yield return "!0z! 0z &a ";
			yield return $"!0z!{CharStore.GetCharRange("0") + "0"} &a ";
			yield return $"!{CharStore.GetCharRange("0") + "0"}!0z &a ";

//			// Nonspecific tag
			yield return "!";
		}

		private static IEnumerable<string> getAnchorTagNegativeTestCases()
		{
			yield return "&a";
			yield return "a ";
			yield return "& a ";
			yield return "&[ ";
			yield return "& ";
			yield return $"&{CharStore.GetCharRange("a") + "a"} ";
		}

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