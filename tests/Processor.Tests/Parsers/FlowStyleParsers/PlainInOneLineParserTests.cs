using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class PlainInOneLineParserTests
	{
		[TestCaseSource(nameof(getKeyContexts))]
		public async Task Process_KeyContextWithPlainOneLineValue_ReturnsValueAndAdvancesStream(Context context)
		{
			const string plainOneLine = "abc def";
			var line = $"{plainOneLine} # comment";
			var stream = createStream(line);

			var result = await createParser().TryProcess(stream, context);

			Assert.That(result?.Value, Is.EqualTo(plainOneLine));
			A.CallTo(() => stream.Read((uint) plainOneLine.Length)).MustHaveHappenedOnceExactly();
		}

		[TestCaseSource(nameof(getKeyContexts))]
		public async Task Process_KeyContextWithoutPlainOneLineValue_ReturnsNullAndDoesNotAdvanceStream(Context context)
		{
			const string? line = "# comment";
			var stream = createStream(line);

			var result = await createParser().TryProcess(stream, context);

			Assert.Null(result);
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getInAndOutContexts))]
		public void Process_NotKeyContext_Throws(Context context)
		{
			var stream = createStream();

			Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => createParser().TryProcess(stream, context).AsTask());
			stream.AssertNotAdvanced();
		}

		[Test]
		public async Task Process_AsOneLine_OnlyPartOfLineParsed_ReturnsNull()
		{
			var stream = createStream("key: value");

			var result = await createParser().TryProcess(stream, Context.BlockKey, asOneLine: true);

			Assert.Null(result);
		}

		[Test]
		public async Task Process_AsOneLine_WholeLineParsed_ReturnsParsedValueAndAdvancesStream()
		{
			var plainOneLineValue = "plain value\n";
			var stream = createStream(plainOneLineValue);

			var result = await createParser().TryProcess(stream, Context.BlockKey, asOneLine: true);

			Assert.That(result?.Value, Is.EqualTo(plainOneLineValue[..^1]));
			A.CallTo(() => stream.Read((uint) plainOneLineValue.Length)).MustHaveHappenedOnceExactly();
		}

		private static ICharacterStream createStream(string line = "")
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.PeekLine()).Returns(line);

			return stream;
		}

		private static PlainInOneLineParser createParser() => new();

		private static IEnumerable<Context> getInAndOutContexts() =>
			Enum.GetValues<Context>().Except(getKeyContexts());

		private static IEnumerable<Context> getKeyContexts()
		{
			yield return Context.BlockKey;
			yield return Context.FlowKey;
		}
	}
}
