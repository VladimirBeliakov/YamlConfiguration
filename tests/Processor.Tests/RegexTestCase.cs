namespace YamlConfiguration.Processor.Tests
{
	public class RegexTestCase
	{
		public string TestValue { get; }
		public string WholeMatch { get; }
		public string[]? Captures { get; }

		public RegexTestCase(string testValue, string wholeMatch, params string[]? captures)
		{
			TestValue = testValue;
			WholeMatch = wholeMatch;
			Captures = captures;
		}
	}
}