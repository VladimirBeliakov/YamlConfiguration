using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class DirectiveParserTests
	{
		[Test]
		public async Task Process_NoDirectiveCharAtBeginning_ReturnsNoDirectivesAndNoDirectiveEndPresent()
		{
			var stream = createStream('a');

			var (directives, isDirectiveEndPresent) = await createParser().Process(stream);

			Assert.Multiple(() =>
				{
					CollectionAssert.IsEmpty(directives);
					Assert.False(isDirectiveEndPresent);
				}
			);
		}

		[Test]
		public async Task Process_StreamWithDirective_ReturnsDirective()
		{
			var stream = createStream();
			var directive = A.Dummy<IDirective>();
			var directiveParser = createDirectiveParser(directive);

			var (directives, _) = await createParser(directiveParser).Process(stream);

			CollectionAssert.AreEqual(new[] { directive }, directives);
		}

		[Test]
		public async Task Process_StreamWithDifferentDirectives_ReturnsAllDirectives()
		{
			var stream = createStream();
			var directive1 = A.Dummy<IDirective>();
			var directive2 = A.Dummy<IDirective>();
			var directiveParser1 = createDirectiveParser(directive1, directive2);
			var directive3 = A.Dummy<IDirective>();
			var directive4 = A.Dummy<IDirective>();
			var directiveParser2 = createDirectiveParser(directive3, directive4);

			var (directives, _) = await createParser(directiveParser1, directiveParser2).Process(stream);

			CollectionAssert.AreEquivalent(new[] { directive1, directive2, directive3, directive4 }, directives);
		}

		[TestCase(new[] { '-', '-', '-', '\n' })]
		[TestCase(new[] { '-', '-', '-', '\n', 'a' })]
		public async Task Process_StreamWithDirectiveEnd_ReturnsDirectiveEndPresent(char[] directiveEnd)
		{
			var stream = createStream(directiveEnd: directiveEnd);

			var (_, isDirectiveEndPresent) = await createParser().Process(stream);

			Assert.True(isDirectiveEndPresent);
		}

		[TestCase(new[] { 'a' })]
		[TestCase(new[] { '\n' })]
		[TestCase(new[] { '-', '\n' })]
		[TestCase(new[] { '-', '-', '\n' })]
		[TestCase(new[] { '-', '-', '-' })]
		[TestCase(new[] { '-', '-', '-', 'a' })]
		[TestCase(new[] { 'a', '-', '-', '\n' })]
		[TestCase(new[] { '-', 'a', '-', '\n' })]
		[TestCase(new[] { '-', '-', 'a', '\n' })]
		public async Task Process_StreamWithoutDirectiveEnd_ReturnsDirectiveEndNotPresent(char[] directiveEnd)
		{
			var stream = createStream(directiveEnd: directiveEnd);

			var (_, isDirectiveEndPresent) = await createParser().Process(stream);

			Assert.False(isDirectiveEndPresent);
		}

		[Test]
		public async Task Process_StreamWithDirectiveEnd_ReadsStreamForeTimes()
		{
			var stream = createStream(directiveEnd: new[] { '-', '-', '-', '\n' });

			await createParser().Process(stream);

			A.CallTo(() => stream.Read()).MustHaveHappened(4, Times.Exactly);
		}

		[Test]
		public async Task Process_StreamWithoutDirectiveEnd_DoesNotReadStream()
		{
			var stream = createStream(directiveEnd: new[] { '-', '-', '\n' });

			await createParser().Process(stream);

			A.CallTo(() => stream.Read()).MustNotHaveHappened();
		}

		private static ICharacterStream createStream(char beginningChar = '%', char[]? directiveEnd = null)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.Peek()).Returns(beginningChar);

			if (directiveEnd is not null)
				A.CallTo(() => stream.Peek(A<int>._)).Returns(directiveEnd);

			return stream;
		}

		private static IOneDirectiveParser createDirectiveParser(params IDirective[] directives)
		{
			var directiveParser = A.Fake<IOneDirectiveParser>();

			if (directives.Length == 0)
				return directiveParser;

			var firstDirective = directives.First();

			var thenConfiguration =
				A.CallTo(() => directiveParser.Process(A<ICharacterStream>._)).Returns(firstDirective).Once();

			foreach (var directive in directives.Skip(1))
				thenConfiguration = thenConfiguration.Then.Returns(directive).Once();

			thenConfiguration.Then.Returns(null);

			return directiveParser;
		}

		private static DirectivesParser createParser(params IOneDirectiveParser?[] directiveParsers)
		{
			var parsers = directiveParsers.Select(
				p =>
				{
					return p ?? A.Fake<IOneDirectiveParser>(
						options => options.ConfigureFake(parser =>
							A.CallTo(() => parser.Process(A<ICharacterStream>._)).Returns(null)
						)
					);
				}
			).ToList();

			return new DirectivesParser(
				parsers,
				A.Dummy<IOneLineCommentParser>()
			);
		}
	}
}