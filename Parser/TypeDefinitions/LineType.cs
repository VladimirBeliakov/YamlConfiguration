using System;

namespace Parser.TypeDefinitions
{
	internal enum LineType : byte
	{
		Scalar,
		ScalarToScalarMapping,
		Sequence,
		MappingOfMappings,
		MappingBeginning,
		ComplexMappingKey,
		SequenceBeginning,
		FlowSequence,
		FlowMapping,
		DocumentBeginning,
		DocumentEnd,
		AnchoredNode,
		AliasedNode,
		NestedElement,
	}
}