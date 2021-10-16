using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	public class BlockFlowTestCase
	{
		public Context Type { get; }
		public string TestValue { get; }
		public string WholeCapture { get; }

		public BlockFlowTestCase(Context type, string testValue, string wholeCapture)
		{
			Type = type;
			TestValue = testValue;
			WholeCapture = wholeCapture;
		}
	}
}