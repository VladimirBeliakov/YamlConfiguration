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
		public async Task Process_PrefixDoesNotSpecifyEncoding_ReturnsDocumentWithUTF8()
		{
			var documentPrefixParser = A.Fake<IDocumentPrefixParser>();
			A.CallTo(() => documentPrefixParser.Process(A<ICharacterStream>._)).Returns(null);

			var document = await createDocumentParser(documentPrefixParser: documentPrefixParser).Process(_charStream);

			Assert.That(document!.Encoding, Is.EqualTo(Encoding.UTF8));
		}

		[TestCase("UTF-32")]
		[TestCase("ASCII")]
		public async Task Process_PrefixSpecifiesEncoding_ReturnsDocumentWithSpecifiedEncoding(string encodingName)
		{
			var encoding = Encoding.GetEncoding(encodingName);
			var documentPrefixParser = A.Fake<IDocumentPrefixParser>();
			A.CallTo(() => documentPrefixParser.Process(A<ICharacterStream>._)).Returns(encoding);

			var document = await createDocumentParser(documentPrefixParser: documentPrefixParser).Process(_charStream);

			Assert.That(document!.Encoding, Is.EqualTo(encoding));
		}

		[Test]
		public void Process_WithDirectivesAndWithoutDirectiveEnd_ThrowsNoDirectiveEnd()
		{
			var directiveParser = A.Fake<IDirectiveParser>();
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(new[] { create<Directive>() }, isDirectiveEndPresent: false)
			);

			var documentParser = createDocumentParser(directiveParser: directiveParser);

			Assert.ThrowsAsync<NoDirectiveEndException>(() => documentParser.Process(_charStream).AsTask());
		}

		[Test]
		public async Task Process_WithoutNodesAndWithoutDirectiveEnd_ReturnsNull()
		{
			var nodeParser = A.Fake<INodeParser>();
			var directiveParser = A.Fake<IDirectiveParser>();
			A.CallTo(() => nodeParser.Process(A<ICharacterStream>._)).Returns(Array.Empty<INode>());
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(Array.Empty<Directive>(), isDirectiveEndPresent: true)
			);

			var documentParser = createDocumentParser(directiveParser: directiveParser, nodeParser: nodeParser);
			var document = await documentParser.Process(_charStream);

			Assert.Null(document);
		}

		[Test]
		public void Process_WithoutNodesAndWithDirectiveEnd_ThrowsNoNodes()
		{
			var nodeParser = A.Fake<INodeParser>();
			A.CallTo(() => nodeParser.Process(A<ICharacterStream>._)).Returns(Array.Empty<INode>());

			var documentParser = createDocumentParser(nodeParser: nodeParser);

			Assert.ThrowsAsync<NoNodesException>(() => documentParser.Process(_charStream).AsTask());
		}

		[Test]
		public async Task Process_WithDirectivesAndWithDirectiveEnd_ReturnsDirectiveDocument()
		{
			var directiveParser = A.Fake<IDirectiveParser>();
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(new[] { create<Directive>() }, isDirectiveEndPresent: true)
			);

			var document = await createDocumentParser(directiveParser: directiveParser).Process(_charStream);

			Assert.That(document!.Type, Is.EqualTo(DocumentType.Directive));
		}

		[Test]
		public async Task Process_WithoutDirectivesAndWithDirectiveEnd_ReturnsExplicitDocument()
		{
			var directiveParser = A.Fake<IDirectiveParser>();
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(Array.Empty<Directive>(), isDirectiveEndPresent: true)
			);

			var document = await createDocumentParser(directiveParser: directiveParser).Process(_charStream);

			Assert.That(document!.Type, Is.EqualTo(DocumentType.Explicit));
		}

		[Test]
		public async Task Process_WithoutDirectivesAndWithoutDirectiveEnd_ReturnsBareDocument()
		{
			var directiveParser = A.Fake<IDirectiveParser>();
			A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(
				new DirectiveParseResult(Array.Empty<Directive>(), isDirectiveEndPresent: false)
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
			var directives = new[] { create<Directive>() };
			var nodeParser = A.Fake<INodeParser>();
			var directiveParser = A.Fake<IDirectiveParser>();
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
			IDirectiveParser? directiveParser = null,
			INodeParser? nodeParser = null,
			IDocumentSuffixParser? documentSuffixParser = null
		)
		{
			var defaultNodeParser = A.Fake<INodeParser>();
			A.CallTo(() => defaultNodeParser.Process(_charStream)).Returns(new[] { A.Dummy<INode>() });

			return new DocumentParser(
				documentPrefixParser ?? A.Dummy<IDocumentPrefixParser>(),
				directiveParser ?? A.Dummy<IDirectiveParser>(),
				nodeParser ?? defaultNodeParser,
				documentSuffixParser ?? A.Dummy<IDocumentSuffixParser>()
			);
		}

		private static T create<T>() => _fixture.Create<T>();
	}
}