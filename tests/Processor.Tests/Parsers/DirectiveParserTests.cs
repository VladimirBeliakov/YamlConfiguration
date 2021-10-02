using System;
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
		public async Task Process_NoDirectiveParsers_ReturnsNoDirectivesAndNoDirectiveEndPresent()
		{
			var stream = createStream();

			var (directives, isDirectiveEndPresent) =
				await createDirectiveParser(Array.Empty<IOneDirectiveParser>()).Process(stream);

			Assert.Multiple(() =>
				{
					CollectionAssert.IsEmpty(directives);
					Assert.False(isDirectiveEndPresent);
				}
			);
		}

		[Test]
		public async Task Process_AllOneDirectiveParsersReturnNull_ReturnsNoDirectivesAndNoDirectiveEndPresent()
		{
			var stream = createStream();
			var oneDirectiveParser1 = createOneDirectiveParser(directives: new IDirective?[] { null });
			var oneDirectiveParser2 = createOneDirectiveParser(directives: new IDirective?[] { null });
			var directiveParser = createDirectiveParser(oneDirectiveParser1, oneDirectiveParser2);

			var (directives, isDirectiveEndPresent) = await directiveParser.Process(stream);

			Assert.Multiple(() =>
				{
					CollectionAssert.IsEmpty(directives);
					Assert.False(isDirectiveEndPresent);
				}
			);
		}

		[Test]
		public async Task Process_OneDirectiveParserReturnsDirective_ReturnsDirective()
		{
			var stream = createStream();
			var directive = A.Dummy<IDirective>();
			var directiveParser = createOneDirectiveParser(directive);

			var (directives, _) = await createDirectiveParser(directiveParser).Process(stream);

			CollectionAssert.AreEqual(new[] { directive }, directives);
		}

		[Test]
		public async Task Process_OneDirectiveParsersReturnDifferentDirectives_ReturnsAllDirectives()
		{
			var stream = createStream();
			var directive1 = A.Dummy<IDirective>();
			var directive2 = A.Dummy<IDirective>();
			var directiveParser1 = createOneDirectiveParser(directive1, directive2);
			var directive3 = A.Dummy<IDirective>();
			var directive4 = A.Dummy<IDirective>();
			var directiveParser2 = createOneDirectiveParser(directive3, directive4);

			var (directives, _) = await createDirectiveParser(directiveParser1, directiveParser2).Process(stream);

			CollectionAssert.AreEquivalent(new[] { directive1, directive2, directive3, directive4 }, directives);
		}

		[TestCase(new[] { '-', '-', '-', '\n' })]
		[TestCase(new[] { '-', '-', '-', '\n', 'a' })]
		public async Task Process_StreamWithDirectiveEnd_ReturnsDirectiveEndPresent(char[] directiveEnd)
		{
			var stream = createStream(directiveEnd: directiveEnd);

			var (_, isDirectiveEndPresent) = await createDirectiveParser().Process(stream);

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

			var (_, isDirectiveEndPresent) = await createDirectiveParser().Process(stream);

			Assert.False(isDirectiveEndPresent);
		}

		[Test]
		public async Task Process_StreamWithDirectiveEnd_ReadsStreamForeTimes()
		{
			var stream = createStream(directiveEnd: new[] { '-', '-', '-', '\n' });

			await createDirectiveParser().Process(stream);

			A.CallTo(() => stream.Read()).MustHaveHappened(4, Times.Exactly);
		}

		[Test]
		public async Task Process_StreamWithoutDirectiveEnd_DoesNotReadStream()
		{
			var stream = createStream(directiveEnd: new[] { '-', '-', '\n' });

			await createDirectiveParser().Process(stream);

			A.CallTo(() => stream.Read()).MustNotHaveHappened();
		}

		private static ICharacterStream createStream(char[]? directiveEnd = null)
		{
			var stream = A.Fake<ICharacterStream>();

			if (directiveEnd is not null)
				A.CallTo(() => stream.Peek(A<uint>._)).Returns(directiveEnd);

			return stream;
		}

		private static IOneDirectiveParser createOneDirectiveParser(params IDirective?[] directives)
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

		private static DirectivesParser createDirectiveParser(params IOneDirectiveParser[] directiveParsers) =>
			new(directiveParsers);
	}
}