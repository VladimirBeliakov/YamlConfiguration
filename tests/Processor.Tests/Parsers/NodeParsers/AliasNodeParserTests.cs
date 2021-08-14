using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests.NodeParsers
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class AliasNodeParserTests
	{
		[Test]
		public async Task Process_FirstCharNotAliasChar_ReturnsNull()
		{
			var stream = createStream("abc");

			var result = await createParser().Process(stream);

			Assert.Null(result);
		}

		[Test]
		public void Process_InvalidAnchorName_Throws()
		{
			var stream = createStream("*[");

			Assert.ThrowsAsync<InvalidYamlException>(() => createParser().Process(stream).AsTask());
		}

		[Test]
		public async Task Process_ValidAlias_ReturnsAliasNodeWithCorrectAnchorName()
		{
			const string anchorName = "abc";
			var stream = createStream($"*{anchorName}");

			var result = (AliasNode?) await createParser().Process(stream);

			Assert.That(result?.Name, Is.EqualTo(anchorName));
		}

		private static ICharacterStream createStream(string chars)
		{
			var stream = A.Fake<ICharacterStream>();

			A.CallTo(() => stream.Peek()).Returns(chars.First());

			A.CallTo(() => stream.ReadLine()).Returns(chars);

			return stream;
		}

		private AliasNodeParser createParser() => new();
	}
}