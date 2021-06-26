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
		}

		[Test]
		public async Task Process_ProcessSpecificReturnsNull_ReturnsNull()
		{
			const string directiveName = "abc";
			var stream = createStream(directiveName: directiveName.ToCharArray());
			var directiveParser = createDirectiveParser(directive: null, directiveName);

			var result = await directiveParser.Process(stream);

			Assert.Null(result);
		}

		[Test]
		public async Task Process_ProcessSpecificReturnsDirective_ReturnsSameDirective()
		{
			const string directiveName = "abc";
			var stream = createStream(directiveName: directiveName.ToCharArray());
			var directive = A.Dummy<IDirective>();
			var directiveParser = createDirectiveParser(directive, directiveName);

			var actualDirective = await directiveParser.Process(stream);

			Assert.That(actualDirective, Is.EqualTo(directive));
		}

		[Test]
		public async Task Process_ProcessSpecificReturnsNull_CommentParserNotCalled()
		{
			const string directiveName = "abc";
			var stream = createStream(directiveName: directiveName.ToCharArray());
			var commentParser = A.Fake<IOneLineCommentParser>();
			var directiveParser = createDirectiveParser(directive: null, directiveName, commentParser);

			await directiveParser.Process(stream);

			A.CallTo(() => commentParser.TryProcess(stream)).MustNotHaveHappened();
		}

		[Test]
		public async Task Process_ProcessSpecificReturnsDirective_CommentParserCalled()
		{
			const string directiveName = "abc";
			var stream = createStream(directiveName: directiveName.ToCharArray());
			var commentParser = A.Fake<IOneLineCommentParser>();
			var directiveParser = createDirectiveParser(A.Dummy<IDirective>(), directiveName, commentParser);

			await directiveParser.Process(stream);

			A.CallTo(() => commentParser.TryProcess(stream)).MustHaveHappened();
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
			IOneLineCommentParser? commentParser = null
		) =>
			new(directive, directiveName, commentParser);

		private class TestOneDirectiveParser : OneDirectiveParser
		{
			private readonly IDirective? _directive;

			public TestOneDirectiveParser(
				IDirective? directive,
				string? directiveName,
				IOneLineCommentParser? commentParser = null
			)
				: base(commentParser ?? A.Dummy<IOneLineCommentParser>())
			{
				_directive = directive;
				DirectiveName = directiveName ?? "TestDirective";
			}

			protected override ValueTask<IDirective?> ProcessSpecific(ICharacterStream charStream) =>
				ValueTask.FromResult(_directive);

			protected override string DirectiveName { get; }
		}
	}
}