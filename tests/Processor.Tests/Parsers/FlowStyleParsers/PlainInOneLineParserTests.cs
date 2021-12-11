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
		[TestCaseSource(nameof(getInLineContexts))]
		public async Task TryProcess_InLineContextAndStreamWithPlainOneLineValue_ReturnsParsedValue(
			Context context
		)
		{
			const string plainOneLine = "abc def";
			var line = $"{plainOneLine} # comment";
			var stream = createStream(line);

			var result = await createParser().TryProcess(stream, context);

			Assert.That(result?.Value, Is.EqualTo(plainOneLine));
			A.CallTo(() => stream.Read((uint) plainOneLine.Length)).MustHaveHappenedOnceExactly();
		}

		[TestCaseSource(nameof(getInLineContexts))]
		public async Task TryProcess_InLineContextAndStreamWithoutPlainOneLineValue_ReturnsNull(
			Context context
		)
		{
			const string? line = "# comment";
			var stream = createStream(line);

			var result = await createParser().TryProcess(stream, context);

			Assert.Null(result);
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getInvalidContexts))]
		public void TryProcess_InvalidContext_Throws(Context context)
		{
			var stream = createStream();

			Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
				createParser().TryProcess(stream, context).AsTask()
			);
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getWholeLineContexts))]
		public async Task TryProcess_WholeLineContext_OnlyPartOfLineParsed_ReturnsNull(Context context)
		{
			var stream = createStream("key: value");

			var result = await createParser().TryProcess(stream, context);

			Assert.Null(result);
		}

		[TestCaseSource(nameof(getWholeLineContexts))]
		public async Task TryProcess_WholeLineContext_WholeLineParsed_ReturnsParsedValue(Context context)
		{
			var plainOneLineValue = "plain value\n";
			var stream = createStream(plainOneLineValue);

			var result = await createParser().TryProcess(stream, context);

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

		private static IEnumerable<Context> getInvalidContexts() =>
			Enum.GetValues<Context>().Except(getInLineContexts()).Except(getWholeLineContexts());

		private static IEnumerable<Context> getInLineContexts()
		{
			yield return Context.BlockKey;
			yield return Context.FlowKey;
		}

		private static IEnumerable<Context> getWholeLineContexts()
		{
			yield return Context.FlowOut;
			yield return Context.FlowIn;
		}
	}
}
