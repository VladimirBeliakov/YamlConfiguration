namespace Processor.FlowStyles
{
	public static class SimpleStyles
	{
		public static string AliasNode =
			$"{BasicStructures.SeparateInLine}" +
			$"{Characters.Alias}{BasicStructures.NodeTags.AnchorName}" +
			$"{BasicStructures.Break}";

		// Note: There can be empty nodes, specifically, node's property and content both can be empty.
		// Empty nodes usually get resolved to null.
	}
}