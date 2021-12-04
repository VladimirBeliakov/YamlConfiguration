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
	public class PlainNextLineParserTests
	{
		[TestCaseSource(nameof(getNotFlowContexts))]
		public void TryProcess_NotFlowContext_Throws(Context context)
		{
			var stream = createStreamFrom("");

			Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
				new PlainNextLineParser().TryProcess(stream, context).AsTask()
			);
		}

		[TestCaseSource(nameof(getFlowContexts))]
		public async Task TryProcess_NotPlainNextLine_ReturnsNull(Context context)
		{
			var stream = createStreamFrom(": some value\n");

			var result = await new PlainNextLineParser().TryProcess(stream, context);

			Assert.Null(result);
		}

		[TestCaseSource(nameof(getFlowContexts))]
		public async Task TryProcess_PlainNextLine_ReturnsParsedLine(Context context)
		{
			const string expectedValue = "some value";
			var stream = createStreamFrom($"{expectedValue}\n");

			var result = await new PlainNextLineParser().TryProcess(stream, context);

			Assert.That(result, Is.EqualTo(expectedValue));
		}

		private static ICharacterStream createStreamFrom(string value)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.PeekLine()).Returns($"{value}");

			return stream;
		}

		private static IEnumerable<Context> getNotFlowContexts() =>
			Enum.GetValues<Context>().Except(getFlowContexts());

		private static IEnumerable<Context> getFlowContexts()
		{
			yield return Context.FlowIn;
			yield return Context.FlowOut;
		}
	}
}
