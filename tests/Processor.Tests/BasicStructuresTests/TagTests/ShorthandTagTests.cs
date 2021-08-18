using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class ShorthandTagTests
	{
		[TestCaseSource(nameof(getShorthandTagPositiveTestCases))]
		public void ValidShorthandTags_MatchesRegex(RegexTestCase testCase)
		{
			var match = _shorthandTagRegex.Match(testCase.TestValue);

			Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));

			var hasShorthandTagGroupBeenCaptured = match.Groups.Count == 2;
			Assert.True(hasShorthandTagGroupBeenCaptured, $"Found {match.Groups.Count} groups.");

			var capturedShorthandTagCount = match.Groups[1].Captures.Count;
			Assert.That(capturedShorthandTagCount, Is.EqualTo(1), $"Found {capturedShorthandTagCount} captures.");

			var capturedShorthandTag = match.Groups[1].Captures[0].Value;
			Assert.That(capturedShorthandTag, Is.EqualTo(testCase.Captures?.FirstOrDefault()));
		}

		[TestCaseSource(nameof(getShorthandTagNegativeTestCases))]
		public void InvalidShorthandTag_DoesNotMatch(string testCase)
		{
			var match = _shorthandTagRegex.Match(testCase);

			Assert.False(match.Success);
		}

		private static IEnumerable<RegexTestCase> getShorthandTagPositiveTestCases()
		{
			var chars = CharStore.Chars;

			var shorthandTags = getShorthandTags();

			foreach (var shorthandTag in shorthandTags)
				yield return new RegexTestCase(
					testValue: shorthandTag + $" {chars}",
					wholeMatch: shorthandTag,
					shorthandTag
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

		private static IEnumerable<string> getShorthandTagNegativeTestCases()
		{
			// Invalid tag handle
			yield return "0!0z ";
			yield return "!%!0z ";
			yield return $"!{CharStore.GetCharRange("0") + "0"}!0 ";

			// Invalid tag char
			yield return "!0! ";
			yield return "!0!0% ";
			yield return "!%0 ";
			yield return "!%a ";
			yield return "!^ ";
			yield return "!, ";
			yield return "![ ";
			yield return "!] ";
			yield return "!{ ";
			yield return "!} ";
			yield return $"!!{CharStore.GetCharRange("0") + "0"} ";

			// No space, tab, or break at the end
			yield return "!0z!0z";
		}

		private static readonly Regex _shorthandTagRegex = new (
			BasicStructures.NodeTags.ShorthandTag,
			RegexOptions.Compiled
		);
	}
}