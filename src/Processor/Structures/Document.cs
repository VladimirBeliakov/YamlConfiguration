using System.Collections.Generic;
using System.Text;

namespace YamlConfiguration.Processor
{
	internal class Document
	{
		public Document(
			DocumentType type,
			IReadOnlyCollection<Directive> directives,
			IReadOnlyCollection<INode> nodes,
			bool withSuffix
		)
		{
			Type = type;
			Directives = directives;
			Nodes = nodes;
			WithSuffix = withSuffix;
		}

		public DocumentType Type { get; }

		public IReadOnlyCollection<Directive> Directives { get; }

		public IReadOnlyCollection<INode> Nodes { get; }

		public bool WithSuffix { get; }
	}
}