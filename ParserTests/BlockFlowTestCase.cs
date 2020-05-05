using Parser.TypeDefinitions;

namespace ParserTests
{
	public class BlockFlowTestCase
	{
		public BlockFlowInOut Type { get; }
		public string Value { get; }
		public string WholeCapture { get; }
		public string FirstParenthesisCapture { get; }
		public string SecondParenthesisCapture { get; }
		public string ThirdParenthesisCapture { get; }
		public string ForthParenthesisCapture { get; }

		public BlockFlowTestCase(
			BlockFlowInOut type,
			string value,
			string wholeCapture,
			string firstParenthesisCapture = null,
			string secondParenthesisCapture = null,
			string thirdParenthesisCapture = null,
			string forthParenthesisCapture = null
		)
		{
			Type = type;
			Value = value;
			WholeCapture = wholeCapture;
			FirstParenthesisCapture = firstParenthesisCapture;
			SecondParenthesisCapture = secondParenthesisCapture;
			ThirdParenthesisCapture = thirdParenthesisCapture;
			ForthParenthesisCapture = forthParenthesisCapture;
		}
	}
}