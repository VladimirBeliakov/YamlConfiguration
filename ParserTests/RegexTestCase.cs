using System.Collections.Generic;

namespace ParserTests
{
	public class RegexTestCase
	{
		public string TestCase { get; }
		public string WholeMatch { get; }
		public string[] Captures { get; }

		public RegexTestCase(string testCase, string wholeMatch, params string[] captures)
		{
			TestCase = testCase;
			WholeMatch = wholeMatch;
			Captures = captures;
		}
	}
}