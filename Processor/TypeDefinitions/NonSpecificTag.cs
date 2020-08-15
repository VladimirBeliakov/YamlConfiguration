namespace Processor.TypeDefinitions
{
	internal enum NonSpecificTag : byte
	{
		ForNonPlainScalars = 0, // represented by '!'
		ForOtherNodes = 1		// represented by '?'
	}
}