using System;

namespace YamlConfiguration.Processor
{
	internal class FoldedLineResult
	{
		public FoldedLineResult(uint emptyLineCount, bool isBreakAsSpace = false)
		{
			if (isBreakAsSpace && emptyLineCount > 0)
				throw new ArgumentException(
						$"{nameof(isBreakAsSpace)} can't be true " +
						$"when {nameof(emptyLineCount)} is greater than 0."
					);

			EmptyLineCount = emptyLineCount;
			IsBreakAsSpace = isBreakAsSpace;
		}

		public uint EmptyLineCount { get; private set; }
		public bool IsBreakAsSpace { get; private set; }
	}
}
