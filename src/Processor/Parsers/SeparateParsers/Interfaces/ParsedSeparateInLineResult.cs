using System;

namespace YamlConfiguration.Processor
{
	internal record ParsedSeparateInLineResult
	{
		public ParsedSeparateInLineResult(bool isSeparateInLine, uint whiteSpaceCount)
		{
			if (!isSeparateInLine && whiteSpaceCount > 0)
				throw new ArgumentException(
					$"{nameof(whiteSpaceCount)} cannot be greater than 0 when {nameof(isSeparateInLine)} is false."
				);

			IsSeparateInLine = isSeparateInLine;
			WhiteSpaceCount = whiteSpaceCount;
		}

		public bool IsSeparateInLine { get; init; }
		public uint WhiteSpaceCount { get; init; }

		public void Deconstruct(out bool isSeparateInLine, out uint whiteSpaceCount)
		{
			isSeparateInLine = IsSeparateInLine;
			whiteSpaceCount = WhiteSpaceCount;
		}
	}
}