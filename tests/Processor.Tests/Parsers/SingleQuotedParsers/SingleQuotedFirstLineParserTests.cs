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
	public class SingleQuotedFirstLineParserTests
	{
		[TestCaseSource(nameof(_invalidContexts))]
		public void Process_InvalidContexts_Throws(Context context)
		{
			var charStream = A.Dummy<ICharacterStream>();

			Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => createParser().Process(charStream, context));
		}

		[TestCaseSource(nameof(_validContexts))]
		public async Task Process_InvalidFirstLine_ReturnsNull(Context context)
		{
			var charStream = createStreamFrom("abc");

			var result = await createParser().Process(charStream, context);

			Assert.Null(result);
		}

		[Test]
		public async Task Process_FirstLineWithoutClosingQuote_ReturnsNotClosedFirstLine(
			[ValueSource(nameof(_validContexts))] Context context,
			[Values("", "abc")] string contentInQuotes
		)
		{
			var firstLineContent = $"'{contentInQuotes}";
			var allChars = $"{firstLineContent}  \ndef";
			var charStream = createStreamFrom(allChars);

			var result = await createParser().Process(charStream, context);

			Assert.Multiple(() =>
				{
					Assert.That(result?.Value, Is.EqualTo(contentInQuotes));
					Assert.False(result?.IsClosed);
				}
			);
			A.CallTo(() => charStream.Read((uint) firstLineContent.Length)).MustHaveHappenedOnceExactly();
		}

		[Test]
		public async Task Process_FirstLineWithClosingQuote_ReturnsClosedFirstLine(
			[ValueSource(nameof(_validContexts))] Context context,
			[Values("  ", "abc  ")] string contentInQuotes
		)
		{
			var firstLineContent = $"'{contentInQuotes}'";
			var allChars = $"{firstLineContent}: def";
			var charStream = createStreamFrom(allChars);

			var result = await createParser().Process(charStream, context);

			Assert.Multiple(() =>
				{
					Assert.That(result?.Value, Is.EqualTo(contentInQuotes));
					Assert.True(result?.IsClosed);
				}
			);
			A.CallTo(() => charStream.Read((uint) firstLineContent.Length)).MustHaveHappenedOnceExactly();
		}

		private static ICharacterStream createStreamFrom(string line = "")
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.PeekLine()).Returns(line);

			return stream;
		}


		private static SingleQuotedFirstLineParser createParser() => new();

		private static readonly IEnumerable<Context> _validContexts = new[] { Context.FlowIn, Context.FlowOut };

		private static readonly IEnumerable<Context> _invalidContexts =
			Enum.GetValues<Context>().Except(_validContexts);
	}
}
