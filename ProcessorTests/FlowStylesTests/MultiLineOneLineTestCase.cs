using Processor.FlowStyles;

namespace ProcessorTests
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