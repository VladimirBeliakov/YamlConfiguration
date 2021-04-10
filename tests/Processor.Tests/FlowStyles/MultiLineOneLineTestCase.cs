using YamlConfiguration.Processor.FlowStyles;

namespace YamlConfiguration.Processor.Tests
{
	public class MultiLineOneLineTestCase
	{
		public MultiLineOneLineTestCase(string testValue, ProcessedLineResult result)
		{
			TestValue = testValue;
			Result = result;
		}

		public string TestValue { get; }
		public ProcessedLineResult Result { get; }
	}
}