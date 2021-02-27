using System.Collections.Generic;

namespace YamlConfiguration.Processor.Tests
{
	public class MultiLineTestCase
	{
		public MultiLineTestCase(MultiLineOneLineTestCase firstLine, params MultiLineOneLineTestCase[] nextLines)
		{
			FirstLine = firstLine;
			NextLines = nextLines;
		}

		public MultiLineOneLineTestCase FirstLine { get; }

		public IEnumerable<MultiLineOneLineTestCase> NextLines { get; }
	}
}