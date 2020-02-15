namespace Parser.TypeDefinitions
{
	internal enum BlockScalarStyle : byte
	{
		Literal = 0, // denoted by |, preserves new lines
		Folded = 1   // denoted by >, new lines become spaces unless breaks end with empty new lines or
					 // more indented new lines
	}
}