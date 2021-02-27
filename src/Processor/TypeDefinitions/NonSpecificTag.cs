namespace YamlConfiguration.Processor.TypeDefinitions
{
	internal enum NonSpecificTag : byte
	{
		ForNonPlainScalars = 1, // represented by '!'
		ForOtherNodes = 2,		// represented by '?'
	}
}