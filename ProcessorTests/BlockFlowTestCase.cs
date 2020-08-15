using Processor.TypeDefinitions;

namespace ProcessorTests
{
	public class BlockFlowTestCase
	{
		public BlockFlow Type { get; }
		public string TestValue { get; }
		public string WholeCapture { get; }

		public BlockFlowTestCase(BlockFlow type, string testValue, string wholeCapture)
		{
			Type = type;
			TestValue = testValue;
			WholeCapture = wholeCapture;
		}
	}
}