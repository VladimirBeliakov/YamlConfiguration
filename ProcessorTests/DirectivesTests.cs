using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Processor;
using ProcessorTests.Extensions;

namespace ProcessorTests
{
	[TestFixture, Parallelizable(ParallelScope.Children)]
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
			var comments = getComments().ToList();

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
			var comments = getComments().ToList();

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
			var tagHandleCases = getTagHandles().ToList();
			var tagPrefixCases = getLocalTagPrefixes().Concat(getGlobalTagPrefixes()).ToList();
			var commentCases = getComments().ToList();

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
			// Wrong tag handle
			yield return "%TAG e! !%11 #comment" + @break;
			yield return "%TAG ? !%11 #comment" + @break;
			yield return "%TAG !? !%11 #comment" + @break;
			yield return "%TAG !?! !%11 #comment" + @break;
			yield return "%TAG #!e! !%11 #comment" + @break;
			// Wrong tag prefix
			yield return "%TAG !e! }%11 #comment" + @break;
			yield return "%TAG !e! !%1 #comment" + @break;
			yield return "%TAG !e! !` #comment" + @break;
			// Wrong separate in line
			yield return "%TAG!e! !%11 #comment" + @break;
			yield return "%TAG !e!!%11 #comment" + @break;
			// Wrong comment
			yield return "%TAG !e! !%11# comment" + @break;
			// No line break
			yield return "%TAG !e! !%11 #comment";
		}

		private static IEnumerable<string> getTagHandles()
		{
			yield return "!";
			yield return "!!";

			foreach (var wordChar in _wordChars.GroupBy(Characters.CharGroupLength))
			{
				yield return $"!{wordChar}";
				yield return $"!{wordChar}!";
			}
		}

		private static IEnumerable<string> getLocalTagPrefixes()
		{
			yield return "!";

			foreach (var uriCharGroup in getUriCharGroups())
				yield return "!" + uriCharGroup;
		}

		private static IEnumerable<string> getComments()
		{
			var @break = Environment.NewLine;
			var chars = CharCache.Chars;

			return new[] { String.Empty + @break, $" #{@break}", $" #{chars + chars}{@break}" };
		}

		private static IEnumerable<string> getGlobalTagPrefixes()
		{
			var notAllowedFirstChars = new[] { "!", ",", "[", "]", "{", "}" };
			var uriChars = getUriCharsWithoutHexNumbers().Concat(getHexNumbers());

			var allowedFirstChars = uriChars.Except(notAllowedFirstChars).ToList();
			var uriCharGroups = getUriCharGroups().ToList();

			var anyUriCharGroup = uriCharGroups.First();
			var anyAllowedChar = allowedFirstChars.First();

			foreach (var allowedFirstChar in allowedFirstChars)
				yield return allowedFirstChar + anyUriCharGroup;

			foreach (var uriCharGroup in uriCharGroups)
				yield return anyAllowedChar + uriCharGroup;
		}

		private static IEnumerable<string> getUriCharGroups()
		{
			var hexNumberGroups = getHexNumbers().GroupBy(Characters.CharGroupLength);
			var uriCharGroupsWithoutHexNumbers = getUriCharsWithoutHexNumbers().GroupBy(Characters.CharGroupLength);

			return hexNumberGroups.Concat(uriCharGroupsWithoutHexNumbers);
		}

		private static IEnumerable<string> getHexNumbers()
		{
			var hexDigits = _hexLetters.Concat(_decimalDigits).ToList();

			return from firstHexDigit in hexDigits
				   from secondHexDigit in hexDigits
				   select "%" + firstHexDigit + secondHexDigit;
		}

		private static IEnumerable<string> getUriCharsWithoutHexNumbers()
		{
			var additionalUriChar = new[]
			{
				"#", ";", "/", "?", ":", "@", "&", "=", "+", "$", ",", "_", ".", "!", "~", "*", "'", "(", ")",
				"[", "]", "”"
			};

			return additionalUriChar.Concat(_wordChars);
		}

		private static readonly IEnumerable<string> _decimalDigits =
			new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

		private static readonly IEnumerable<string> _hexLetters =
			new[] { "A", "B", "C", "D", "E", "F", "a", "b", "c", "d", "e", "f" };

		private static readonly IEnumerable<string> _asciiLetters = _hexLetters.Concat(
			new[]
			{
				"G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
				"g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
			}
		);

		private static readonly IEnumerable<string> _wordChars = _decimalDigits.Concat(_asciiLetters);

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