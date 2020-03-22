using Parser.TypeDefinitions;

namespace DeserializerTests
{
	public class BlockFlowTestCase
	{
		public BlockFlowInOut Type { get; }
		public string Value { get; }
		public string WholeCapture { get; }
		public string ParenthesisCapture { get; }

		public BlockFlowTestCase(
			BlockFlowInOut type,
			string value,
			string wholeCapture,
			string parenthesisCapture = null
		)
		{
			Type = type;
			Value = value;
			WholeCapture = wholeCapture;
			ParenthesisCapture = parenthesisCapture;
		}
	}
}