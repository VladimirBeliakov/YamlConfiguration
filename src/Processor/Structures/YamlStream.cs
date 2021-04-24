using System.Collections.Generic;

namespace YamlConfiguration.Processor
{
	internal class YamlStream
	{
		private readonly List<Document> _documents = new List<Document>();

		public IReadOnlyCollection<Document> Documents => _documents;

		public void Add(Document document) => _documents.Add(document);
	}
}