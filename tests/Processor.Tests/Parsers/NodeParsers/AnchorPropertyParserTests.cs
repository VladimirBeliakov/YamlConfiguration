using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests.NodeParsers
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class AnchorPropertyParserTests
	{
		[Test]
		public async Task Process_StreamDoesNotBeginWithAnchorChar_ReturnsNull()
		{
			var stream = createStreamFrom(new[] { 'a' });

			var result = await createParser().Process(stream);

			Assert.Null(result);
		}

		[TestCaseSource(nameof(getWhiteSpacesAndBreak))]
		public async Task Process_StreamWithValidAnchorProperty_ReturnsAnchorPropertyWithAnchorName(
			char whiteSpaceOrBreak
		)
		{
			var stream = createStreamFrom(new[] { '&', 'a', 'b', 'c', whiteSpaceOrBreak });

			var result = await createParser().Process(stream);

			Assert.That(result?.AnchorName, Is.EqualTo("abc"));
		}

		[Test]
		public async Task Process_StreamWithValidAnchorPropertyButNoWhiteSpaceOrBreakCharAfterwards_ReturnsNull()
		{
			var stream = createStreamFrom(new[] { '&', 'a', 'b', 'c' });

			var result = await createParser().Process(stream);

			Assert.Null(result);
		}

		[Test]
		public async Task Process_StreamWithValidAnchorProperty_AdvancesStream()
		{
			var stream = createStreamFrom(new[] { '&', 'a', 'b', 'c', ' ' });

			await createParser().Process(stream);

			A.CallTo(() => stream.Read(4)).MustHaveHappenedOnceExactly();
		}

		private static ICharacterStream createStreamFrom(char[] chars)
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

		private static AnchorPropertyParser createParser() => new();
	}
}