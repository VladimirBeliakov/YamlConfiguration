using System.Collections.Generic;

namespace ProcessorTests
{
	public class MultiLineTestCase
	{
		public MultiLineTestCase(OneLineTestCase firstLine, params OneLineTestCase[] nextLines)
		{
			FirstLine = firstLine;
			NextLines = nextLines;
		}

		public OneLineTestCase FirstLine { get; }

		public IEnumerable<OneLineTestCase> NextLines { get; }
	}
}