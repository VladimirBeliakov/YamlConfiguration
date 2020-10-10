using Processor.FlowStyles;

namespace ProcessorTests
{
	public class OneLineTestCase
	{
		public OneLineTestCase(string testValue, ProcessedLineResult result)
		{
			TestValue = testValue;
			Result = result;
		}

		public string TestValue { get; }
		public ProcessedLineResult Result { get; }
	}
}