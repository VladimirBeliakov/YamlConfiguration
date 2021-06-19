using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using YamlConfiguration.Processor;

namespace Sandbox
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class YamlStreamParserTests
	{
		[Test]
		public async Task Process_StreamReturnsNoDocument_ReturnsNull()
		{
			var documentParser = A.Fake<IDocumentParser>();
			A.CallTo(() => documentParser.Process(A<ICharacterStream>._)).Returns(null);

			var yamlStreamParser = new YamlStreamParser(documentParser);
			var yamlStream = await yamlStreamParser.Process(A.Dummy<ICharacterStream>());

			Assert.Null(yamlStream);
		}

		[TestCaseSource(nameof(_notExplicitDocumentTypes))]
		public void Process_PreviousDocumentWithoutSuffixAndCurrentNotExplicit_Throws(DocumentType documentType)
		{
			var documentParser = A.Fake<IDocumentParser>();
			var documentWithoutSuffix = createDocument(withSuffix: false);
			var notExplicitDocument = createDocument(type: documentType);
			A.CallTo(() => documentParser.Process(A<ICharacterStream>._))
				.Returns(documentWithoutSuffix).Once().Then
				.Returns(notExplicitDocument).Once().Then
				.Returns(null);

			var yamlStreamParser = new YamlStreamParser(documentParser);

			Assert.ThrowsAsync<InvalidOperationException>(() =>
				yamlStreamParser.Process(A.Dummy<ICharacterStream>()).AsTask()
			);
		}

		[Test]
		public void Process_PreviousDocumentWithoutSuffixAndCurrentExplicit_DoesNotThrow()
		{
			var documentParser = A.Fake<IDocumentParser>();
			var documentWithoutSuffix = createDocument(withSuffix: false);
			var explicitDocument = createDocument(type: DocumentType.Explicit);
			A.CallTo(() => documentParser.Process(A<ICharacterStream>._))
				.Returns(documentWithoutSuffix).Once().Then
				.Returns(explicitDocument).Once().Then
				.Returns(null);

			var yamlStreamParser = new YamlStreamParser(documentParser);

			Assert.DoesNotThrow(() => yamlStreamParser.Process(A.Dummy<ICharacterStream>()).AsTask());
		}

		[Test]
		public void Process_PreviousDocumentWithSuffixAndCurrentOfAnyType_DoesNotThrow(
			[Values] DocumentType documentType
		)
		{
			var documentParser = A.Fake<IDocumentParser>();
			var documentWithoutSuffix = createDocument(withSuffix: true);
			var explicitDocument = createDocument(type: documentType);
			A.CallTo(() => documentParser.Process(A<ICharacterStream>._))
				.Returns(documentWithoutSuffix).Once().Then
				.Returns(explicitDocument).Once().Then
				.Returns(null);

			var yamlStreamParser = new YamlStreamParser(documentParser);

			Assert.DoesNotThrow(() => yamlStreamParser.Process(A.Dummy<ICharacterStream>()).AsTask());
		}

		[Test]
		public async Task Process_ReturnsDocumentsInSameOrderAsInStream()
		{
			var documentParser = A.Fake<IDocumentParser>();
			var document1 = createDocument();
			var document2 = createDocument();
			A.CallTo(() => documentParser.Process(A<ICharacterStream>._))
				.Returns(document1).Once().Then
				.Returns(document2).Once().Then
				.Returns(null);

			var yamlStreamParser = new YamlStreamParser(documentParser);
			var yamlStream = await yamlStreamParser.Process(A.Dummy<ICharacterStream>());

			Assert.That(yamlStream?.Documents.Count, Is.EqualTo(2));
			Assert.Multiple(() =>
				{
					var firstDocument = yamlStream!.Documents.First();
					var secondDocument = yamlStream.Documents.Skip(1).First();

					Assert.AreSame(document1, firstDocument);
					Assert.AreSame(document2, secondDocument);
				}
			);
		}

		private static Document createDocument(DocumentType type = DocumentType.Bare, bool withSuffix = true) =>
			new Document(type, Array.Empty<IDirective>(), Array.Empty<INode>(), withSuffix);

		private static IReadOnlyCollection<DocumentType> _notExplicitDocumentTypes =
			Enum.GetValues<DocumentType>().Where(dt => dt != DocumentType.Explicit).ToList();
	}
}