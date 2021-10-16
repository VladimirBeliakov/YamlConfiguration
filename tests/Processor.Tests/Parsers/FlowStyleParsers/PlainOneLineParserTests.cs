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
	public class PlainOneLineParserTests
	{
		[TestCaseSource(nameof(getKeyContext))]
		public async Task Process_KeyContextWithPlainOneLineValue_ReturnsValueAndAdvancesStream(Context context)
		{
			const string plainOneLine = "abc def";
			var line = $"{plainOneLine} # comment";
			var stream = createStream(line);

			var result = await createParser().Process(stream, context);

			Assert.That(result?.Value, Is.EqualTo(plainOneLine));
			A.CallTo(() => stream.Read((uint) plainOneLine.Length)).MustHaveHappenedOnceExactly();
		}

		[TestCaseSource(nameof(getKeyContext))]
		public async Task Process_KeyContextWithoutPlainOneLineValue_ReturnsNullAndDoesNotAdvanceStream(Context context)
		{
			const string? line = "# comment";
			var stream = createStream(line);

			var result = await createParser().Process(stream, context);

			Assert.Null(result);
			stream.AssertNotAdvanced();
		}

		[TestCaseSource(nameof(getInAndOutContext))]
		public void Process_NotKeyContext_Throws(Context context)
		{
			var stream = createStream();

			Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => createParser().Process(stream, context).AsTask());
			stream.AssertNotAdvanced();
		}

		private static ICharacterStream createStream(string line = "")
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.PeekLine()).Returns(line);

			return stream;
		}

		private static PlainOneLineParser createParser() => new();

		private static IEnumerable<Context> getInAndOutContext() => Enum.GetValues<Context>().Except(getKeyContext());

		private static IEnumerable<Context> getKeyContext()
		{
			yield return Context.BlockKey;
			yield return Context.FlowKey;
		}
	}
}