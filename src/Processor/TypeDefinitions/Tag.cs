namespace YamlConfiguration.Processor.TypeDefinitions
{
	internal enum Tag : byte
	{
		Sequence = 1,
		Mapping = 2,
		String = 3,
		Integer = 4,
		Float = 5,
		Null = 6,
		Binary = 7,
		Omap = 8,
		Set = 9,
		Timestamp = 10,
		Global = 11,
	}
}