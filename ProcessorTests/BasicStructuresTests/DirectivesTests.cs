using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Processor;

namespace ProcessorTests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class DirectivesTests
	{
		[TestCaseSource(nameof(getReservedDirectiveTestCases))]
		public void ReservedDirective_ValidDirective_Matches(RegexTestCase testCase)
		{
			var match = _reservedDirectiveRegex.Match(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
					Assert.That(match.Groups.Count, Is.EqualTo(3));
					Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.Captures[0]));
					Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[2].Captures[0].Value, Is.EqualTo(testCase.Captures[1]));
				}
			);
		}

		[TestCaseSource(nameof(getReservedDirectiveUnmatchableTestCases))]
		public void ReservedDirective_InvalidDirective_DoesNotMatch(string testCase)
		{
			var match = _reservedDirectiveRegex.Match(testCase);

			Assert.False(match.Success);
		}

		[TestCaseSource(nameof(getYamlDirectiveTestCases))]
		public void YamlDirective_ValidDirective_Matches(RegexTestCase testCase)
		{
			var match = _yamlDirectiveRegex.Match(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
					Assert.That(match.Groups.Count, Is.EqualTo(2));
					Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.Captures[0]));
				}
			);
		}

		[TestCaseSource(nameof(getYamlDirectiveUnmatchableTestCases))]
		public void YamlDirective_InvalidDirective_DoesNotMatch(string testCase)
		{
			var match = _yamlDirectiveRegex.Match(testCase);

			Assert.False(match.Success);
		}

		[TestCaseSource(nameof(getTagDirectiveTestCases))]
		public void TagDirective_ValidDirective_Matches(RegexTestCase testCase)
		{
			var match = _tagDirectiveRegex.Match(testCase.TestValue);

			Assert.Multiple(
				() =>
				{
					Assert.That(match.Value, Is.EqualTo(testCase.WholeMatch));
					Assert.That(match.Groups.Count, Is.EqualTo(3));
					Assert.That(match.Groups[1].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[1].Captures[0].Value, Is.EqualTo(testCase.Captures[0]));
					Assert.That(match.Groups[2].Captures.Count, Is.EqualTo(1));
					Assert.That(match.Groups[2].Captures[0].Value, Is.EqualTo(testCase.Captures[1]));
				}
			);
		}

		[TestCaseSource(nameof(getTagDirectiveUnmatchableTestCases))]
		public void TagDirective_InvalidDirective_DoesNotMatch(string testCase)
		{
			var match = _tagDirectiveRegex.Match(testCase);

			Assert.False(match.Success);
		}

		private static IEnumerable<RegexTestCase> getReservedDirectiveTestCases()
		{
			var chars = CharCache.Chars;
			var directiveNames = new[] { "A", chars };
			var separateInLines = CharCache.SeparateInLineCases;
			var directiveParameters = new[] { "A", chars };
			var comments = CharCache.GetComments().ToList();

			var anyDirectiveName = directiveNames.First();
			var anySeparateInLine = separateInLines.First();
			var anyDirectiveParameter = directiveParameters.First();
			var anyComment = comments.First();

			foreach (var directiveName in directiveNames)
			{
				yield return new RegexTestCase(
					testValue: "%" + directiveName + anySeparateInLine + anyDirectiveParameter + anyComment + chars,
					wholeMatch: "%" + directiveName + anySeparateInLine + anyDirectiveParameter + anyComment,
					directiveName,
					anyDirectiveParameter
				);
			}

			foreach (var separateInLine in separateInLines)
			{
				yield return new RegexTestCase(
					testValue: "%" + anyDirectiveName + separateInLine + anyDirectiveParameter + anyComment + chars,
					wholeMatch: "%" + anyDirectiveName + separateInLine + anyDirectiveParameter + anyComment,
					anyDirectiveName,
					anyDirectiveParameter
				);
			}

			foreach (var directiveParameter in directiveParameters)
			{
				yield return new RegexTestCase(
					testValue: "%" + anyDirectiveName + anySeparateInLine + directiveParameter + anyComment + chars,
					wholeMatch: "%" + anyDirectiveName + anySeparateInLine + directiveParameter + anyComment,
					anyDirectiveName,
					directiveParameter
				);
			}

			foreach (var comment in comments)
			{
				yield return new RegexTestCase(
					testValue: "%" + anyDirectiveName + anySeparateInLine + anyDirectiveParameter + comment + chars,
					wholeMatch: "%" + anyDirectiveName + anySeparateInLine + anyDirectiveParameter + comment,
					anyDirectiveName,
					anyDirectiveParameter
				);
			}
		}

		private static IEnumerable<string> getReservedDirectiveUnmatchableTestCases()
		{
			var @break = Environment.NewLine;
			var tooLongCharRange = CharCache.Chars + "a";

			yield return $"% A A{@break}";
			yield return $" %A A{@break}";
			yield return $"%A A A{@break}";
			yield return $"%A{@break}";
			yield return $"% {@break}";
			yield return $"%{@break}";
			yield return $"A A{@break}";
			yield return $"%{tooLongCharRange} A{@break}";
			yield return $"%A {tooLongCharRange}{@break}";
		}

		private static IEnumerable<RegexTestCase> getYamlDirectiveTestCases()
		{
			var chars = CharCache.Chars;
			var separateInLines = CharCache.SeparateInLineCases;
			var digits = CharCache.Digits;
			var comments = CharCache.GetComments().ToList();

			var anySeparateInLine = separateInLines.First();
			var anyDigit = digits.First();
			var anyComment = comments.First();

			foreach (var separateInLine in separateInLines)
			{
				var version = anyDigit + "." + anyDigit;
				yield return new RegexTestCase(
					"%YAML" + separateInLine + version + anyComment + chars,
					"%YAML" + separateInLine + version + anyComment,
					version
				);
			}

			foreach (var digit in digits)
			{
				var version = digit + "." + digit;
				yield return new RegexTestCase(
					"%YAML" + anySeparateInLine + version + anyComment + chars,
					"%YAML" + anySeparateInLine + version + anyComment,
					version
				);
			}

			foreach (var comment in comments)
			{
				var version = anyDigit + "." + anyDigit;
				yield return new RegexTestCase(
					"%YAML" + anySeparateInLine + version + comment + chars,
					"%YAML" + anySeparateInLine + version + comment,
					version
				);
			}
		}

		private static IEnumerable<string> getYamlDirectiveUnmatchableTestCases()
		{
			var @break = Environment.NewLine;
			var tooLongDigitRange = CharCache.Digits + "a";

			yield return $"YAML 1.2{@break}";
			yield return $"%AML 1.2{@break}";
			yield return $"% YAML 1.2{@break}";
			yield return $"%YAML .2{@break}";
			yield return $"%YAML 12{@break}";
			yield return $"%YAML 1.{@break}";
			yield return $"%YAML1.2{@break}";
			yield return $"%YAML {@break}";
			yield return $"%YAML{@break}";
			yield return $"%YAML {tooLongDigitRange}.2{@break}";
			yield return $"%YAML 1.{tooLongDigitRange}{@break}";
		}

		private static IEnumerable<RegexTestCase> getTagDirectiveTestCases()
		{
			var tagHandleCases = CharCache.GetTagHandles().ToList();
			var tagPrefixCases = getLocalTagPrefixes().Concat(getGlobalTagPrefixes()).ToList();
			var commentCases = CharCache.GetComments().ToList();

			var anySeparateInLine = CharCache.SeparateInLineCases.First();
			var anyTagHandle = tagHandleCases.First();
			var anyTagPrefix = tagPrefixCases.First();
			var anyComment = commentCases.First();

			foreach (var separateInLine in CharCache.SeparateInLineCases)
			{
				yield return new RegexTestCase(
					testValue: "%TAG" + separateInLine + anyTagHandle + separateInLine + anyTagPrefix + anyComment,
					wholeMatch: "%TAG" + separateInLine + anyTagHandle + separateInLine + anyTagPrefix + anyComment,
					anyTagHandle,
					anyTagPrefix
				);
			}

			foreach (var tagHandle in tagHandleCases)
			{
				yield return new RegexTestCase(
					testValue: "%TAG" + anySeparateInLine + tagHandle + anySeparateInLine + anyTagPrefix + anyComment,
					wholeMatch: "%TAG" + anySeparateInLine + tagHandle + anySeparateInLine + anyTagPrefix + anyComment,
					tagHandle,
					anyTagPrefix
				);
			}

			foreach (var tagPrefix in tagPrefixCases)
			{
				yield return new RegexTestCase(
					testValue: "%TAG" + anySeparateInLine + anyTagHandle + anySeparateInLine + tagPrefix + anyComment,
					wholeMatch: "%TAG" + anySeparateInLine + anyTagHandle + anySeparateInLine + tagPrefix + anyComment,
					anyTagHandle,
					tagPrefix
				);
			}

			foreach (var comment in commentCases)
				yield return new RegexTestCase(
					testValue: "%TAG" + anySeparateInLine + anyTagHandle + anySeparateInLine + anyTagPrefix + comment,
					wholeMatch: "%TAG" + anySeparateInLine + anyTagHandle + anySeparateInLine + anyTagPrefix + comment,
					anyTagHandle,
					anyTagPrefix
				);
		}

		private static IEnumerable<string> getTagDirectiveUnmatchableTestCases()
		{
			var @break = Environment.NewLine;
			// Wrong %TAG
			yield return "TAG !e! !%11 #comment" + @break;
			yield return "%AG !e! !%11 #comment" + @break;
			yield return " !e! !%11 #comment" + @break;
			// Wrong tag handle
			yield return "%TAG e! !%11 #comment" + @break;
			yield return "%TAG ? !%11 #comment" + @break;
			yield return "%TAG !? !%11 #comment" + @break;
			yield return "%TAG !?! !%11 #comment" + @break;
			yield return "%TAG #!e! !%11 #comment" + @break;
			yield return "%TAG !%11 #comment" + @break;
			// Wrong tag prefix
			yield return "%TAG !e! }%11 #comment" + @break;
			yield return "%TAG !e! !%1 #comment" + @break;
			yield return "%TAG !e! !` #comment" + @break;
			yield return "%TAG !e!" + @break;
			// Wrong separate in line
			yield return "%TAG!e! !%11 #comment" + @break;
			yield return "%TAG !e!!%11 #comment" + @break;
			// Wrong comment
			yield return "%TAG !e! !%11# comment" + @break;
			// No line break
			yield return "%TAG !e! !%11 #comment";
		}

		private static IEnumerable<string> getLocalTagPrefixes()
		{
			yield return "!";

			foreach (var uriCharGroup in CharCache.GetUriCharGroups())
				yield return "!" + uriCharGroup;
		}

		private static IEnumerable<string> getGlobalTagPrefixes()
		{
			var tagChars = CharCache.GetTagChars().ToList();
			var uriCharGroups = CharCache.GetUriCharGroups().ToList();

			var anyUriCharGroup = uriCharGroups.First();
			var anyTagChar = tagChars.First();

			foreach (var allowedFirstChar in tagChars)
				yield return allowedFirstChar + anyUriCharGroup;

			foreach (var uriCharGroup in uriCharGroups)
				yield return anyTagChar + uriCharGroup;
		}

		private static readonly Regex _reservedDirectiveRegex = new Regex(
			BasicStructures.Directives.Reserved,
			RegexOptions.Compiled
		);

		private static readonly Regex _yamlDirectiveRegex = new Regex(
			BasicStructures.Directives.Yaml,
			RegexOptions.Compiled
		);

		private static readonly Regex _tagDirectiveRegex = new Regex(
			BasicStructures.Directives.Tag,
			RegexOptions.Compiled
		);
	}
}