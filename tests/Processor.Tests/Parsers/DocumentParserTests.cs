using System;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture]
	public class DocumentParserTests
	{
		private static readonly ICharacterStream _charStream = A.Dummy<ICharacterStream>();
		private static readonly Fixture _fixture = new Fixture();

		[Test]
		public void Process_WithDirectivesAndWithoutDirectiveEnd_ThrowsNoDirectiveEnd()
		{
			var directiveParser = A.Fake<IDirectivesParser>();
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(new[] { create<IDirective>() }, isDirectiveEndPresent: false)
			);

			var documentParser = createDocumentParser(directiveParser: directiveParser);

			Assert.ThrowsAsync<NoDirectiveEndException>(() => documentParser.Process(_charStream).AsTask());
		}

		[Test]
		public async Task Process_WithoutNodesAndWithoutDirectiveEnd_ReturnsNull()
		{
			var nodeParser = A.Fake<INodeParser>();
			var directiveParser = A.Fake<IDirectivesParser>();
			A.CallTo(() => nodeParser.Process(A<ICharacterStream>._)).Returns(Array.Empty<INode>());
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(Array.Empty<IDirective>(), isDirectiveEndPresent: false)
			);

			var documentParser = createDocumentParser(directiveParser: directiveParser, nodeParser: nodeParser);
			var document = await documentParser.Process(_charStream);

			Assert.Null(document);
		}

		[Test]
		public void Process_WithoutNodesAndWithDirectiveEnd_ThrowsNoNodes()
		{
			var nodeParser = A.Fake<INodeParser>();
			var directiveParser = A.Fake<IDirectivesParser>();
			A.CallTo(() => nodeParser.Process(A<ICharacterStream>._)).Returns(Array.Empty<INode>());
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(Array.Empty<IDirective>(), isDirectiveEndPresent: true)
			);

			var documentParser = createDocumentParser(nodeParser: nodeParser, directiveParser: directiveParser);

			Assert.ThrowsAsync<NoNodesException>(() => documentParser.Process(_charStream).AsTask());
		}

		[Test]
		public async Task Process_WithDirectivesAndWithDirectiveEnd_ReturnsDirectiveDocument()
		{
			var directiveParser = A.Fake<IDirectivesParser>();
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(new[] { create<IDirective>() }, isDirectiveEndPresent: true)
			);

			var document = await createDocumentParser(directiveParser: directiveParser).Process(_charStream);

			Assert.That(document!.Type, Is.EqualTo(DocumentType.Directive));
		}

		[Test]
		public async Task Process_WithoutDirectivesAndWithDirectiveEnd_ReturnsExplicitDocument()
		{
			var directiveParser = A.Fake<IDirectivesParser>();
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(Array.Empty<IDirective>(), isDirectiveEndPresent: true)
			);

			var document = await createDocumentParser(directiveParser: directiveParser).Process(_charStream);

			Assert.That(document!.Type, Is.EqualTo(DocumentType.Explicit));
		}

		[Test]
		public async Task Process_WithoutDirectivesAndWithoutDirectiveEnd_ReturnsBareDocument()
		{
			var directiveParser = A.Fake<IDirectivesParser>();
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(Array.Empty<IDirective>(), isDirectiveEndPresent: false)
			);

			var document = await createDocumentParser(directiveParser: directiveParser).Process(_charStream);

			Assert.That(document!.Type, Is.EqualTo(DocumentType.Bare));
		}
		[Test]
		public async Task Process_WithOrWithoutSuffix_ReturnsDocumentWithOrWithoutSuffix([Values] bool withSuffix)
		{
			var documentSuffixParser = A.Fake<IDocumentSuffixParser>();
			A.CallTo(() => documentSuffixParser.Process(A<ICharacterStream>._)).Returns(withSuffix);

			var document = await createDocumentParser(documentSuffixParser: documentSuffixParser).Process(_charStream);

			Assert.That(document!.WithSuffix, Is.EqualTo(withSuffix));
		}

		[Test]
		public async Task Process_WithDirectivesAndWithNodes_ReturnsSameDirectivesAndNodes()
		{
			var nodes = new[] { A.Dummy<INode>() };
			var directives = new[] { create<IDirective>() };
			var nodeParser = A.Fake<INodeParser>();
			var directiveParser = A.Fake<IDirectivesParser>();
			A.CallTo(() => nodeParser.Process(A<ICharacterStream>._)).Returns(nodes);
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(directives, isDirectiveEndPresent: true)
			);

			var documentParser = createDocumentParser(directiveParser: directiveParser, nodeParser: nodeParser);
			var document = await documentParser.Process(_charStream);

			Assert.Multiple(() =>
				{
					Assert.AreSame(directives, document!.Directives);
					Assert.AreSame(nodes, document.Nodes);
				}
			);
		}

		private static DocumentParser createDocumentParser(
			IDocumentPrefixParser? documentPrefixParser = null,
			IDirectivesParser? directiveParser = null,
			INodeParser? nodeParser = null,
			IDocumentSuffixParser? documentSuffixParser = null
		)
		{
			var defaultNodeParser = A.Fake<INodeParser>();
			A.CallTo(() => defaultNodeParser.Process(_charStream)).Returns(new[] { A.Dummy<INode>() });

			return new DocumentParser(
				documentPrefixParser ?? A.Dummy<IDocumentPrefixParser>(),
				directiveParser ?? A.Dummy<IDirectivesParser>(),
				nodeParser ?? defaultNodeParser,
				documentSuffixParser ?? A.Dummy<IDocumentSuffixParser>()
			);
		}

		private static T create<T>() => _fixture.Create<T>();
	}
}