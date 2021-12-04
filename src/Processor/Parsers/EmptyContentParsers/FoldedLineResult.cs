using System;

namespace YamlConfiguration.Processor
{
	internal class FoldedLinesResult
	{
		public FoldedLinesResult(uint emptyLineCount, bool isBreakAsSpace = false)
		{
			if ((isBreakAsSpace && emptyLineCount > 0) || (!isBreakAsSpace && emptyLineCount is 0))
				throw new ArgumentException(
						$"{nameof(isBreakAsSpace)} can't be true when {nameof(emptyLineCount)} is greater than 0, " +
						$"and {nameof(emptyLineCount)} can't be 0 when {nameof(isBreakAsSpace)} is false."
					);

			EmptyLineCount = emptyLineCount;
			IsBreakAsSpace = isBreakAsSpace;
		}

		public uint EmptyLineCount { get; private set; }
		public bool IsBreakAsSpace { get; private set; }
	}
}
