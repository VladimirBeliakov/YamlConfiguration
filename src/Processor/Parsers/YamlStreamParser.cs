using System;
using System.Linq;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class YamlStreamParser : IYamlStreamParser
	{
		private readonly IDocumentParser _documentParser;

		public YamlStreamParser(IDocumentParser documentParser)
		{
			_documentParser = documentParser;
		}

		public async ValueTask<YamlStream?> Process(ICharacterStream charStream)
		{
			YamlStream? yamlStream = null;

			while (true)
			{
				var document = await _documentParser.Process(charStream).ConfigureAwait(false);

				if (document is null)
					break;

				yamlStream ??= new YamlStream();

				var isPreviousDocumentWithoutSuffix = yamlStream.Documents.LastOrDefault()?.WithSuffix is false;

				if (isPreviousDocumentWithoutSuffix && document.Type is not DocumentType.Explicit)
					throw new InvalidOperationException(
						"When the previous document has no suffix, the following document can only be explicit."
					);

				yamlStream.Add(document);
			}

			return yamlStream;
		}
	}
}
