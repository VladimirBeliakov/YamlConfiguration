using Parser.TypeDefinitions;

namespace ParserTests
{
	public class BlockFlowTestCase
	{
		public BlockFlowInOut Type { get; }
		public string TestValue { get; }
		public string WholeCapture { get; }

		public BlockFlowTestCase(BlockFlowInOut type, string testValue, string wholeCapture)
		{
			Type = type;
			TestValue = testValue;
			WholeCapture = wholeCapture;
		}
	}
}