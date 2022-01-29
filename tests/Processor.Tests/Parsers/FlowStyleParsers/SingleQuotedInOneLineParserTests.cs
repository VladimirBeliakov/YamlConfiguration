using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class SingleQuotedInOneLineParserTests
	{
		[TestCaseSource(nameof(getInLineContexts))]
		public async Task Process_InvalidValueInKeyContextAsync_ReturnsNull(Context context)
		{
			var invalidValue = "'abc";
			var stream = createStreamFrom(invalidValue);

			var result = await createParser().Process(stream, context);

			Assert.Null(result);
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getInLineContexts))]
		public async Task Process_ValidValueInKeyContextAsync_ReturnsExtractedValue(Context context)
		{
			var extractedValue = "abc";
			var quotedValue = $"'{extractedValue}'";
			var value = $"{quotedValue} def\n";
			var stream = createStreamFrom(value);

			var result = await createParser().Process(stream, context);

			Assert.That(result?.Value, Is.EqualTo(extractedValue));
			A.CallTo(() => stream.Read((uint) quotedValue.Length)).MustHaveHappenedOnceExactly();
		}

		private static ICharacterStream createStreamFrom(string line = "")
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.PeekLine()).Returns(line);

			return stream;
		}

		private static SingleQuotedInOneLineParser createParser() => new();

		private static IEnumerable<Context> getInLineContexts()
		{
			yield return Context.BlockKey;
			yield return Context.FlowKey;
		}
	}
}
