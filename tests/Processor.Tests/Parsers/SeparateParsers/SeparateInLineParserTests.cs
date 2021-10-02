using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class SeparateInLineParserTests
	{
		[Test]
		public async Task Peek_StreamStartsWithNoWhiteSpace_ReturnsWhetherStreamAtStartOfLineAndZeroWhiteSpaces(
			[Values] bool isAtStartOfLine
		)
		{
			var stream = createStreamFrom(new[] { 'a' }, isAtStartOfLine);

			var (isSeparateInLine, whiteSpaceCount) = await createParser().Peek(stream);

			Assert.Multiple(() =>
				{
					Assert.That(isSeparateInLine, Is.EqualTo(isAtStartOfLine));
					Assert.That(whiteSpaceCount, Is.Zero);
				}
			);
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getCharsWithWhiteSpaceCharCount))]
		public async Task Peek_StreamStartsWithWhiteSpace_ReturnsTrueAndCorrectWhiteSpaceCount(
			char[] testCase, int expectedWhiteSpaceCount
		)
		{
			var stream = createStreamFrom(testCase);

			var (isSeparateInLine, whiteSpaceCount) = await createParser().Peek(stream);

			Assert.Multiple(() =>
				{
					Assert.True(isSeparateInLine);
					Assert.That(whiteSpaceCount, Is.EqualTo(expectedWhiteSpaceCount));
				}
			);
			stream.AssertNotAdvanced();
		}

		[Test]
		public void Peek_StreamStartsWithTooManyWhiteSpace_Throws()
		{
			var stream = createStreamFrom(CharStore.GetCharRange(" ").Append(' ').ToArray());

			Assert.ThrowsAsync<InvalidYamlException>(() => createParser().Peek(stream).AsTask());
			stream.AssertNotAdvanced();
		}

		private static ICharacterStream createStreamFrom(char[] chars, bool isAtStartOfLine = false)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.Peek()).Returns(chars[0]);

			A.CallTo(() => stream.Peek(A<uint>._)).Returns(chars);

			A.CallTo(() => stream.IsAtStartOfLine).Returns(isAtStartOfLine);

			return stream;
		}

		private static IEnumerable<TestCaseData> getCharsWithWhiteSpaceCharCount()
		{
			yield return new TestCaseData(new[] { ' ' }, 1);
			yield return new TestCaseData(new[] { ' ', '\t', 'a' }, 2);
			yield return new TestCaseData(new[] { '\t', ' ', 'a' }, 2);
			yield return new TestCaseData(CharStore.GetCharRange(" ").Append('a').ToArray(), 1000);
			yield return new TestCaseData(CharStore.GetCharRange("\t").Append('a').ToArray(), 1000);
		}

		private static SeparateInLineParser createParser() => new();
	}
}