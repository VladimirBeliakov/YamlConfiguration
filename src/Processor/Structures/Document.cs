using System.Collections.Generic;
using System.Text;

namespace YamlConfiguration.Processor
{
	internal class Document
	{
		private readonly Encoding? _encoding;

		private readonly List<Directive> _directives = new List<Directive>();

		private readonly List<INode> _nodes = new List<INode>();

		private readonly Dictionary<string, INode> _anchors = new Dictionary<string, INode>();

		public Document(DocumentType type, bool withSuffix, Encoding? encoding = null)
		{
			_encoding = encoding;
			Type = type;
			WithSuffix = withSuffix;
		}

		public DocumentType Type { get; }

		public bool WithSuffix { get; }

		public void Add(Directive directive) => _directives.Add(directive);

		public void Add(INode node) => _nodes.Add(node);

		public void Add(Anchor anchor) => _anchors.Add(anchor.Name, anchor.Value);

		public INode? TryGetAnchor(string name) => _anchors.TryGetValue(name, out var node) ? node : null;
	}
}