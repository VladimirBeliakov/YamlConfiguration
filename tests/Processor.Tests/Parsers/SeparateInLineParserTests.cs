using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using YamlConfiguration.Processor.SeparateParsers;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class SeparateInLineParserTests
	{
		[Test]
		public async Task TryProcess_StreamStartsWithNoWhiteSpace_ReturnsWhetherStreamAtStartOfLine(
			[Values] bool isAtStartOfLine
		)
		{
			var stream = createStreamFrom(new[] { 'a' }, isAtStartOfLine);

			var result = await createParser().TryProcess(stream);

			Assert.That(result, Is.EqualTo(isAtStartOfLine));
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getCharsWithWhiteSpaceCharCount))]
		public async Task TryProcess_StreamStartsWithWhiteSpace_ReturnsTrueAndAdvancesStreamByWhiteSpaceCharCount(
			char[] testCase, int whiteSpaceCount
		)
		{
			var stream = createStreamFrom(testCase);

			var result = await createParser().TryProcess(stream);

			Assert.True(result);
			A.CallTo(() => stream.Read(whiteSpaceCount)).MustHaveHappenedOnceExactly();
		}

		[Test]
		public void TryProcess_StreamStartsWithTooManyWhiteSpace_Throws()
		{
			var stream = createStreamFrom(CharStore.GetCharRange(" ").Append(' ').ToArray());

			Assert.ThrowsAsync<InvalidYamlException>(() => createParser().TryProcess(stream).AsTask());
			stream.AssertNotAdvanced();
		}

		private static ICharacterStream createStreamFrom(char[] chars, bool isAtStartOfLine = false)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.Peek()).Returns(chars[0]);

			A.CallTo(() => stream.Peek(A<int>._)).Returns(chars);

			A.CallTo(() => stream.IsAtStartOfLine).Returns(isAtStartOfLine);

			return stream;
		}

		private static IEnumerable<TestCaseData> getCharsWithWhiteSpaceCharCount()
		{
			yield return new TestCaseData(new[] { ' ' }, 1);
			yield return new TestCaseData(new[] { ' ', '\t', 'a' }, 2);
			yield return new TestCaseData(new[] { '\t', ' ', 'a' }, 2);
			yield return new TestCaseData(CharStore.GetCharRange(" ").Append('a').ToArray(), 1000);
		}

		private static SeparateInLineParser createParser() => new();
	}
}