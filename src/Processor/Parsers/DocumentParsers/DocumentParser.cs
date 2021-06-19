using System;
using System.Threading.Tasks;

namespace YamlConfiguration.Processor
{
	internal class DocumentParser : IDocumentParser
	{
		private readonly IDocumentPrefixParser _documentPrefixParser;
		private readonly IDocumentSuffixParser _documentSuffixParser;
		private readonly INodeParser _nodeParser;
		private readonly IDirectivesParser _directivesParser;

		public DocumentParser(
			IDocumentPrefixParser documentPrefixParser,
			IDirectivesParser directivesParser,
			INodeParser nodeParser,
			IDocumentSuffixParser documentSuffixParser
		)
		{
			_documentPrefixParser = documentPrefixParser;
			_documentSuffixParser = documentSuffixParser;
			_nodeParser = nodeParser;
			_directivesParser = directivesParser;
		}

		public async ValueTask<Document?> Process(ICharacterStream charStream)
		{
			await _documentPrefixParser.Process(charStream).ConfigureAwait(false);

			var directiveParseResult = await _directivesParser.Process(charStream).ConfigureAwait(false);

			var (directives, isDirectiveEndPresent) = directiveParseResult;

			if (directives.Count > 0 && !isDirectiveEndPresent)
				throw new NoDirectiveEndException("A directive end always has to follow directives.");

			// TODO: Use directives when processing nodes.
			var nodes = await _nodeParser.Process(charStream).ConfigureAwait(false);

			if (nodes.Count == 0)
			{
				if (isDirectiveEndPresent)
					throw new NoNodesException("A directive end must be followed by at least one node.");

				return null;
			}

			DocumentType documentType;

			if (directives.Count > 0)
				documentType = DocumentType.Directive;

			else if (isDirectiveEndPresent)
				documentType = DocumentType.Explicit;

			else
				documentType = DocumentType.Bare;

			var withSuffix = await _documentSuffixParser.Process(charStream).ConfigureAwait(false);

			return new Document(documentType, directives, nodes, withSuffix);
		}
	}
}