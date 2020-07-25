using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Parser;

namespace ParserTests
{
	[TestFixture, Parallelizable(ParallelScope.Children)]
	public class DirectivesTests
	{
		[TestCaseSource(nameof(getReservedDirectiveTestCases))]
		public void ReservedDirective_ValidDirective_Matches(string testCase)
		{
			var match = _reservedDirectiveRegex.Match(testCase);

			Assert.True(match.Success);
		}

		[TestCaseSource(nameof(getReservedDirectiveUnmatchableTestCases))]
		public void ReservedDirective_InvalidDirective_DoesNotMatch(string testCase)
		{
			var match = _reservedDirectiveRegex.Match(testCase);

			Assert.False(match.Success);
		}

		[TestCaseSource(nameof(getYamlDirectiveTestCases))]
		public void YamlDirective_ValidDirective_Matches(string testCase)
		{
			var match = _yamlDirectiveRegex.Match(testCase);

			Assert.True(match.Success);
		}

		[TestCaseSource(nameof(getYamlDirectiveUnmatchableTestCases))]
		public void YamlDirective_InvalidDirective_DoesNotMatch(string testCase)
		{
			var match = _yamlDirectiveRegex.Match(testCase);

			Assert.False(match.Success);
		}

		private static IEnumerable<string> getReservedDirectiveTestCases()
		{
			var chars = CharCache.Chars;
			var @break = Environment.NewLine;

			foreach (var directiveName in new[] { "A", chars })
			{
				foreach (var separateInLine in CharCache.SeparateInLineCases)
				{
					foreach (var directiveParameter in new[] { "A", chars })
					{
						foreach (var comment in new[]
						{
							string.Empty + @break, $" #{@break}", $" #{chars + chars}{@break}"
						})
						{
							yield return "%" + directiveName + separateInLine + directiveParameter + comment;
						}
					}
				}
			}
		}

		private static IEnumerable<string> getReservedDirectiveUnmatchableTestCases()
		{
			var @break = Environment.NewLine;
			yield return $"% A A{@break}";
			yield return $" %A A{@break}";
			yield return $"%A A A{@break}";
			yield return $"%A{@break}";
			yield return $"% {@break}";
			yield return $"%{@break}";
			yield return $"A A{@break}";
		}

		private static IEnumerable<string> getYamlDirectiveTestCases()
		{
			var chars = CharCache.Chars;
			var @break = Environment.NewLine;

			foreach (var separateInLine in CharCache.SeparateInLineCases)
			{
				foreach (var digit in CharCache.Digits)
				{
					foreach (var comment in new[]
					{
						string.Empty + @break, $" #{@break}", $" #{chars + chars}{@break}"
					})
					{
						yield return "%YAML" + separateInLine + digit + "." + digit + comment;
					}
				}
			}
		}

		private static IEnumerable<string> getYamlDirectiveUnmatchableTestCases()
		{
			var @break = Environment.NewLine;

			yield return $"YAML 1.2{@break}";
			yield return $"%AML 1.2{@break}";
			yield return $"% YAML 1.2{@break}";
			yield return $"%YAML .2{@break}";
			yield return $"%YAML 12{@break}";
			yield return $"%YAML 1.{@break}";
			yield return $"%YAML1.2{@break}";
			yield return $"%YAML {@break}";
			yield return $"%YAML{@break}";
		}

		private static readonly Regex _reservedDirectiveRegex = new Regex(Directives.Reserved, RegexOptions.Compiled);
		private static readonly Regex _yamlDirectiveRegex = new Regex(Directives.Yaml, RegexOptions.Compiled);
	}
}