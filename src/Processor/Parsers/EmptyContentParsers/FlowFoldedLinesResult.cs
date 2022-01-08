namespace YamlConfiguration.Processor.FlowStyles
{
	internal class FlowFoldedLinesResult
	{
		public FlowFoldedLinesResult(uint separateInLineWhiteSpaceCount, FoldedLinesResult? foldedLineResult)
		{
			SeparateInLineWhiteSpaceCount = separateInLineWhiteSpaceCount;
			FoldedLineResult = foldedLineResult;
		}

		public uint SeparateInLineWhiteSpaceCount { get; }

		public FoldedLinesResult? FoldedLineResult { get; }
	}
}
