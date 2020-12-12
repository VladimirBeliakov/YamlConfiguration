using System.Linq;

namespace Processor.Tests
{
	public static class Helpers
	{
		public static string RepeatAndJoin(string element, int count) =>
			string.Join(string.Empty, Enumerable.Repeat(element, count));
	}
}