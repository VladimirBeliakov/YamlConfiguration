using Parser.TypeDefinitions;

namespace DeserializerTests
{
	public class BlockFlowTestCase
	{
		public BlockFlowInOut Type { get; }
		public string Value { get; }
		public string WholeCapture { get; }
		public string FirstParenthesisCapture { get; }
		public string SecondParenthesisCapture { get; }

		public BlockFlowTestCase(
			BlockFlowInOut type,
			string value,
			string wholeCapture,
			string firstParenthesisCapture = null,
			string secondParenthesisCapture = null
		)
		{
			Type = type;
			Value = value;
			WholeCapture = wholeCapture;
			FirstParenthesisCapture = firstParenthesisCapture;
			SecondParenthesisCapture = secondParenthesisCapture;
		}
	}
}