using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class OneDirectiveParserTests
	{
		[Test]
		public async Task Process_StreamDoesNotStartWithDirectiveCharacter_ReturnsNull()
		{
			var stream = createStream(directiveChar: 'a');
			var directiveParser = createDirectiveParser();

			var result = await directiveParser.Process(stream);

			Assert.Null(result);
			stream.AssertNotAdvanced();
		}

		[TestCase(new[] { 'a', 'b' })]
		[TestCase(new[] { 'a', 'b', 'd' })]
		public async Task Process_DirectiveNameInStreamDoesNotMatchDirectiveNameInParser_ReturnsNull(
			char[] directiveName
		)
		{
			var stream = createStream(directiveName: directiveName);
			var directiveParser = createDirectiveParser(directiveName: "abc");

			var result = await directiveParser.Process(stream);

			Assert.Null(result);
			stream.AssertNotAdvanced();
		}

		[Test]
		public async Task Process_DirectiveNotParsed_ReturnsNull()
		{
			const string directiveName = "abc";
			var stream = createStream(directiveName: directiveName.ToCharArray());
			var directiveParser = createDirectiveParser(directive: null, directiveName);

			var result = await directiveParser.Process(stream);

			Assert.Null(result);
			A.CallTo(() => stream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[Test]
		public async Task Process_DirectiveParsed_ReturnsSameDirective()
		{
			const string directiveName = "abc";
			var stream = createStream(directiveName: directiveName.ToCharArray());
			var directive = A.Dummy<IDirective>();
			var directiveParser = createDirectiveParser(directive, directiveName);

			var actualDirective = await directiveParser.Process(stream);

			Assert.That(actualDirective, Is.EqualTo(directive));
			A.CallTo(() => stream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[Test]
		public async Task Process_DirectiveNotParsed_MultiLineCommentParserNotCalled()
		{
			const string directiveName = "abc";
			var stream = createStream(directiveName: directiveName.ToCharArray());
			var multiLineCommentParser = A.Fake<IMultiLineCommentParser>();
			var directiveParser = createDirectiveParser(directive: null, directiveName, multiLineCommentParser);

			await directiveParser.Process(stream);

			A.CallTo(() => multiLineCommentParser.TryProcess(stream)).MustNotHaveHappened();
		}

		[Test]
		public async Task Process_DirectiveParsed_MultiLineCommentParserCalled()
		{
			const string directiveName = "abc";
			var stream = createStream(directiveName: directiveName.ToCharArray());
			var multiLineCommentParser = A.Fake<IMultiLineCommentParser>();
			A.CallTo(() => multiLineCommentParser.TryProcess(stream)).Returns(true);
			var directiveParser = createDirectiveParser(A.Dummy<IDirective>(), directiveName, multiLineCommentParser);

			await directiveParser.Process(stream);

			A.CallTo(() => multiLineCommentParser.TryProcess(stream)).MustHaveHappened();
		}

		[Test]
		public void Process_MultiLineCommentParserReturnsFalse_Throws()
		{
			var stream = createStream();
			var multiLineCommentParser = A.Fake<IMultiLineCommentParser>();
			A.CallTo(() => multiLineCommentParser.TryProcess(stream)).Returns(false);

			var directiveParser =
				createDirectiveParser(A.Dummy<IDirective>(), multiLineCommentParser: multiLineCommentParser);
			Assert.ThrowsAsync<InvalidYamlException>(() => directiveParser.Process(stream).AsTask());
		}

		private static ICharacterStream createStream(char directiveChar = '%', char[]? directiveName = null)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.Peek()).Returns(directiveChar);

			if (directiveName is not null)
				A.CallTo(() => stream.Peek(A<int>._)).Returns(new[] { directiveChar }.Concat(directiveName).ToList());

			return stream;
		}

		private static TestOneDirectiveParser createDirectiveParser(
			IDirective? directive = null,
			string? directiveName = null,
			IMultiLineCommentParser? multiLineCommentParser = null
		)
		{
			var defaultMultiLineCommentParser = A.Fake<IMultiLineCommentParser>();
			A.CallTo(() => defaultMultiLineCommentParser.TryProcess(A<ICharacterStream>._)).Returns(true);

			return new(directive, directiveName, multiLineCommentParser ?? defaultMultiLineCommentParser);
		}

		private class TestOneDirectiveParser : OneDirectiveParser
		{
			private readonly IDirective? _directive;

			public TestOneDirectiveParser(
				IDirective? directive,
				string? directiveName,
				IMultiLineCommentParser commentParser
			)
				: base(commentParser)
			{
				_directive = directive;
				DirectiveName = directiveName ?? String.Empty;
			}

			protected override IDirective? Parse(string rawDirective) => _directive;

			protected override string DirectiveName { get; }
		}
	}
}