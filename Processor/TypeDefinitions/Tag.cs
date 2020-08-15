namespace Processor.TypeDefinitions
{
	internal enum Tag : byte
	{
		Sequence = 0,
		Mapping = 1,
		String = 2,
		Integer = 3,
		Float = 4,
		Null = 5,
		Binary = 6,
		Omap = 7,
		Set = 8,
		Timestamp = 9,
		Global = 10
	}
}