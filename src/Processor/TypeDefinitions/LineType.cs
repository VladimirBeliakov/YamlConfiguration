namespace YamlConfiguration.Processor.TypeDefinitions
{
	internal enum LineType : byte
	{
		Scalar = 1,
		ScalarToScalarMapping = 2,
		Sequence = 3,
		MappingOfMappings = 4,
		MappingBeginning = 5,
		ComplexMappingKey = 6,
		SequenceBeginning = 7,
		FlowSequence = 8,
		FlowMapping = 9,
		DocumentBeginning = 10,
		DocumentEnd = 11,
		AnchoredNode = 12,
		AliasedNode = 13,
		NestedElement = 14,
	}
}