namespace YamlConfiguration.Processor.TypeDefinitions
{
	internal enum BlockScalarStyle : byte
	{
		Literal = 1, // denoted by |, preserves new lines
		Folded = 2,  // denoted by >, new lines become spaces unless breaks end with empty new lines or
					 // more indented new lines
	}
}