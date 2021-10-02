using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class FlowLinePrefixParserTests
	{
		[Test]
		public void TryProcess_IndentLengthIsTooHigh_Throws()
		{
			const int invalidIndentLength = 1001;
			var separateInLineParser = A.Fake<ISeparateInLineParser>();
			var stream = createStreamFrom(new[] { ' ' }, invalidIndentLength);

			Assert.ThrowsAsync<InvalidYamlException>(
				() => createParser(separateInLineParser).TryProcess(stream, invalidIndentLength).AsTask()
			);
			A.CallTo(() => separateInLineParser.Peek(stream)).MustNotHaveHappened();
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getCharsWithIndentDifferentFromIndentLength))]
		public async Task TryProcess_StreamStartsWithLessNumberOfIndentCharsThanIndentLength_ReturnsFalse(
			char[] chars,
			int indentLength
		)
		{
			var stream = createStreamFrom(chars, indentLength);
			var separateInLineParser = A.Fake<ISeparateInLineParser>();

			var result = await createParser(separateInLineParser).TryProcess(stream, indentLength);

			Assert.False(result);
			A.CallTo(() => separateInLineParser.Peek(stream)).MustNotHaveHappened();
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getCharsWithIndentSameAsIndentLength))]
		public async Task TryProcess_StreamStartsWithSameNumberOfIndentCharsAsIndentLength_ReturnTrue(
			char[] chars,
			int indentLength
		)
		{
			var stream = createStreamFrom(chars, indentLength);
			var separateInLineParser = A.Fake<ISeparateInLineParser>();

			var result = await createParser(separateInLineParser).TryProcess(stream, indentLength);

			Assert.True(result);
			A.CallTo(() => separateInLineParser.Peek(stream)).MustHaveHappenedOnceExactly();
			A.CallTo(() => stream.Read(indentLength)).MustHaveHappenedOnceExactly();
		}

		private static ICharacterStream createStreamFrom(char[] chars, int indentLength)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.Peek(indentLength)).Returns(chars.Take(indentLength).ToList());

			return stream;
		}

		private static FlowLinePrefixParser createParser(ISeparateInLineParser? separateInLineParser = null)
		{
			return new(separateInLineParser ?? A.Dummy<ISeparateInLineParser>());
		}

		private static IEnumerable<TestCaseData> getCharsWithIndentDifferentFromIndentLength()
		{
			yield return new TestCaseData(new[] { 'a' }, 1);
			yield return new TestCaseData(new[] { ' ', 'a' }, 2);
			yield return new TestCaseData(new[] { ' ' }, 1000);
		}

		private static IEnumerable<TestCaseData> getCharsWithIndentSameAsIndentLength()
		{
			yield return new TestCaseData(new[] { 'a' }, 0);
			yield return new TestCaseData(new[] { ' ' }, 0);
			yield return new TestCaseData(new[] { ' ', 'a' }, 1);
			yield return new TestCaseData(CharStore.GetCharRange(" ").Append('a').ToArray(), 1000);
		}
	}
}