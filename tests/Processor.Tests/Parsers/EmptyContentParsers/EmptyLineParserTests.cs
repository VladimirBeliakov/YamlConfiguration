using System;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class EmptyLineParserTests
	{
		[Test]
		public async Task TryProcess_NoCharsInStream_ReturnsFalse()
		{
			var stream = createStreamFrom(String.Empty);

			var result = await createParser().TryProcess(stream);

			Assert.False(result);
			stream.AssertNotAdvanced();
		}

		[Test]
		public void TryProcess_StreamBeginsWithTab_Throws()
		{
			var stream = createStreamFrom("\tabc");

			Assert.ThrowsAsync<InvalidYamlException>(() => createParser().TryProcess(stream).AsTask());
		}

		[Test]
		public async Task TryProcess_SomeCharsInStreamAreNotWhiteSpaces_ReturnsFalse()
		{
			var stream = createStreamFrom(" \ta \t");

			var result = await createParser().TryProcess(stream);

			Assert.False(result);
			stream.AssertNotAdvanced();
		}

		[Test]
		public async Task TryProcess_NoBreakInTheEnd_ReturnsFalse()
		{
			var stream = createStreamFrom(" \t \t");

			var result = await createParser().TryProcess(stream);

			Assert.False(result);
			stream.AssertNotAdvanced();
		}

		[Test]
		public async Task TryProcess_OnlyBreakInStream_ReturnsTrue()
		{
			var stream = createStreamFrom("\n");

			var result = await createParser().TryProcess(stream);

			Assert.True(result);
			A.CallTo(() => stream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		[Test]
		public async Task TryProcess_AllCharsInStreamAreWhiteSpacesAndBreakInTheEnd_ReturnsTrue()
		{
			var stream = createStreamFrom(" \t \t\n");

			var result = await createParser().TryProcess(stream);

			Assert.True(result);
			A.CallTo(() => stream.ReadLine()).MustHaveHappenedOnceExactly();
		}

		private static ICharacterStream createStreamFrom(string line)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.PeekLine()).Returns(line);

			return stream;
		}

		private static EmptyLineParser createParser() => new();
	}
}
