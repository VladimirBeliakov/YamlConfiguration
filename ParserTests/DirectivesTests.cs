using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Parser;

namespace ParserTests
{
	[TestFixture]
	public class DirectivesTests
	{
		[TestCaseSource(nameof(getReservedDirectiveTestCases))]
		public void ReservedDirective_ValidDirective_Matches(string testCase)
		{
			var match = _reservedDirectiveRegex.Match(testCase);

			Assert.True(match.Success);
		}

		[TestCaseSource(nameof(getReservedDirectiveUnmatchableCases))]
		public void ReservedDirective_InvalidDirective_DoesNotMatch(string testCase)
		{
			var match = _reservedDirectiveRegex.Match(testCase);

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

		private static IEnumerable<string> getReservedDirectiveUnmatchableCases()
		{
			var @break = Environment.NewLine;
			yield return $"% A A{@break}";
			yield return $"%A A A{@break}";
			yield return $"%A{@break}";
			yield return $"% {@break}";
			yield return $"%{@break}";
			yield return $"A A{@break}";
		}

		private static readonly Regex _reservedDirectiveRegex = new Regex(Directives.Reserved, RegexOptions.Compiled);
	}
}