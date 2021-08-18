using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class AnchorPropertyTests
	{
		[TestCaseSource(nameof(getAnchorPositiveTestCases))]
		public void ValidAnchorProperty_MatchesRegex(RegexTestCase testCase)
		{
			var match = _anchorPropertiesRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));

			var hasAnchorPropertyGroupBeenCaptured = match.Groups.Count == 2;
			Assert.True(hasAnchorPropertyGroupBeenCaptured, $"Found {match.Groups.Count} groups.");

			var capturedAnchorPropertyCount = match.Groups[1].Captures.Count;
			Assert.That(capturedAnchorPropertyCount, Is.EqualTo(1), $"Found {capturedAnchorPropertyCount} captures.");

			var capturedAnchorProperty = match.Groups[1].Captures[0].Value;
			Assert.That(capturedAnchorProperty, Is.EqualTo(testCase.Captures?.FirstOrDefault()));
		}

		[TestCaseSource(nameof(getAnchorNegativeTestCases))]
		public void InvalidAnchorProperty_DoesNotMatchRegex(string testCase)
		{
			var match = _anchorPropertiesRegex.Match(testCase);

			Assert.False(match.Success);
		}

		private static IEnumerable<RegexTestCase> getAnchorPositiveTestCases()
		{
			var chars = CharStore.Chars;

			var anchorNames = CharStore.GetNsCharGroups(CharStore.FlowIndicators).ToList();

			foreach (var anchorName in anchorNames)
			{
				var anchorProperty = $"&{anchorName}";

				yield return new RegexTestCase(
					testValue: anchorProperty + $" {chars}",
					wholeMatch: anchorProperty,
					anchorName
				);
			}
		}

		private static IEnumerable<string> getAnchorNegativeTestCases()
		{
			yield return "&a";
			yield return "a ";
			yield return "& a ";
			yield return "&[ ";
			yield return "& ";
			yield return $"&{CharStore.GetCharRange("a") + "a"} ";
		}

		private static readonly Regex _anchorPropertiesRegex = new(
			BasicStructures.NodeTags.AnchorProperty,
			RegexOptions.Compiled
		);
	}
}