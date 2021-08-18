using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class VerbatimTagTests
	{
		[TestCaseSource(nameof(getVerbatimTagPositiveTestCases))]
		public void ValidVerbatimTags_MatchesRegex(RegexTestCase testCase)
		{
			var match = _verbatimTagRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));

			var hasVerbatimTagGroupCaptured = match.Groups.Count == 2;
			Assert.True(hasVerbatimTagGroupCaptured, $"Found {match.Groups.Count} groups.");

			var capturedVerbatimTagCount = match.Groups[1].Captures.Count;
			Assert.That(capturedVerbatimTagCount, Is.EqualTo(1), $"Found {match.Groups[1].Captures.Count} captures.");

			var capturedVerbatimTag = match.Groups[1].Captures[0].Value;
			Assert.That(capturedVerbatimTag, Is.EqualTo(testCase.Captures?.FirstOrDefault()));
		}

		[TestCaseSource(nameof(getVerbatimTagNegativeTestCases))]
		public void InvalidVerbatimTag_DoesNotMatch(string testCase)
		{
			var match = _verbatimTagRegex.Match(testCase);

			Assert.False(match.Success);
		}

		private static IEnumerable<RegexTestCase> getVerbatimTagPositiveTestCases()
		{
			var chars = CharStore.Chars;

			var verbatimTags = CharStore.GetUriCharGroups().Select(g => $"!<{g}>");

			foreach (var verbatimTag in verbatimTags)
				yield return new RegexTestCase(
					testValue: verbatimTag + $" {chars}",
					wholeMatch: verbatimTag,
					verbatimTag
				);
		}

		private static IEnumerable<string> getVerbatimTagNegativeTestCases()
		{
			yield return "!<%0f>";
			yield return "<%0f> ";
			yield return "!%0f> ";
			yield return "!<%0f ";
			yield return "!<%> ";
			yield return "!<%f> ";
			yield return "!< %0f> ";
			yield return "!<%0f > ";
			yield return $"!<{CharStore.GetCharRange("0") + "0"}> ";
		}

		private static readonly Regex _verbatimTagRegex = new (
			BasicStructures.NodeTags.VerbatimTag,
			RegexOptions.Compiled
		);
	}
}