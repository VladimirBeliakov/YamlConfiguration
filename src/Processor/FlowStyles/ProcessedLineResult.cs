namespace YamlConfiguration.Processor.FlowStyles
{
	public class ProcessedLineResult
	{
		public static ProcessedLineResult First(string extractedValue)
		{
			return new ProcessedLineResult(LineType.First, extractedValue);
		}

		public static ProcessedLineResult Empty()
		{
			return new ProcessedLineResult(LineType.Empty, string.Empty);
		}

		public static ProcessedLineResult NotEmpty(string extractedValue)
		{
			return new ProcessedLineResult(LineType.NotEmpty, extractedValue);
		}

		public static ProcessedLineResult LastEmpty()
		{
			return new ProcessedLineResult(LineType.LastEmpty, string.Empty);
		}

		public static ProcessedLineResult LastNotEmpty(string extractedValue)
		{
			return new ProcessedLineResult(LineType.LastNotEmpty, extractedValue);
		}

		public static ProcessedLineResult Invalid()
		{
			return new ProcessedLineResult(LineType.Invalid);
		}

		private ProcessedLineResult(LineType lineType, string extractedValue = null)
		{
			LineType = lineType;
			ExtractedValue = extractedValue;
		}

		public LineType LineType { get; }
		public string ExtractedValue { get; }
	}
}