using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests.NodeParsers
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class TagPropertyParserTests
	{
		[Test]
		public async Task Process_StreamDoesNotBeginWithTagIndicator_ReturnsNull()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStream(charArray);

			var result = await createParser().Process(stream);

			Assert.Null(result);
		}

		[Test]
		public async Task Process_StreamWithVerbatimTagButNoWhiteSpaceOrBreakCharAfterwards_ReturnsNull()
		{
			var charArray = new[] { '!', '<', 'a', '>' };
			var stream = createStream(charArray);

			var result = await createParser().Process(stream);

			Assert.Null(result);
		}

		[TestCaseSource(nameof(getWhiteSpacesAndBreak))]
		public async Task Process_StreamWithVerbatimTag_ReturnsVerbatimTagProperty(char whiteSpaceOrBreak)
		{
			var charArray = new[] { '!', '<', 'a', '>', whiteSpaceOrBreak };
			var stream = createStream(charArray);

			var result = await createParser().Process(stream);

			Assert.Multiple(() =>
				{
					Assert.That(result?.Type, Is.EqualTo(TagType.Verbatim));
					Assert.That(result?.Value, Is.EqualTo("!<a>"));
				}
			);
		}

		[Test]
		public async Task Process_StreamWithShorthandTagButNoWhiteSpaceOrBreakCharAfterwards_ReturnsNull()
		{
			var charArray = new[] { '!', 'a', '!', 'b'  };
			var stream = createStream(charArray);

			var result = await createParser().Process(stream);

			Assert.Null(result);
		}

		[TestCaseSource(nameof(getWhiteSpacesAndBreak))]
		public async Task Process_StreamWithShorthandTag_ReturnsShorthandTagProperty(char whiteSpaceOrBreak)
		{
			var charArray = new[] { '!', 'a', '!', 'b', whiteSpaceOrBreak };
			var stream = createStream(charArray);

			var result = await createParser().Process(stream);

			Assert.Multiple(() =>
				{
					Assert.That(result?.Type, Is.EqualTo(TagType.Shorthand));
					Assert.That(result?.Value, Is.EqualTo("!a!b"));
				}
			);
		}

		[Test]
		public async Task Process_StreamWithNonSpecificTagButNoWhiteSpaceOrBreakCharAfterwards_ReturnsNull()
		{
			var charArray = new[] { '!' };
			var stream = createStream(charArray);

			var result = await createParser().Process(stream);

			Assert.Null(result);
		}

		[TestCaseSource(nameof(getWhiteSpacesAndBreak))]
		public async Task Process_StreamWithNonSpecificTag_ReturnsShorthandTagProperty(char whiteSpaceOrBreak)
		{
			var charArray = new[] { '!', whiteSpaceOrBreak };
			var stream = createStream(charArray);

			var result = await createParser().Process(stream);

			Assert.Multiple(() =>
				{
					Assert.That(result?.Type, Is.EqualTo(TagType.NonSpecific));
					Assert.That(result?.Value, Is.EqualTo("!"));
				}
			);
		}

		[TestCase(new[] { '!' })]
		[TestCase(new[] { '!', 'a', '!', 'b' })]
		[TestCase(new[] { '!', '<', 'a', '>' })]
		public async Task Process_StreamWithValidTag_AdvancesStream(char[] tagChars)
		{
			var stream = createStream(tagChars.Append(' ').ToArray());

			await createParser().Process(stream);

			A.CallTo(() => stream.Read(tagChars.Length)).MustHaveHappenedOnceExactly();
		}

		private static ICharacterStream createStream(char[] chars)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.Peek()).Returns(chars.First());

			var breakIndex = chars.TakeWhile(c => c is not '\n').Count();
			var oneLineChars = breakIndex < chars.Length ? chars.Take(breakIndex + 1).ToArray() : chars;

			A.CallTo(() => stream.PeekLine()).Returns(new String(oneLineChars));

			return stream;
		}

		private static IEnumerable<char> getWhiteSpacesAndBreak()
		{
			yield return ' ';
			yield return '\t';
			yield return '\n';
		}

		private static TagPropertyParser createParser() => new();
	}
}