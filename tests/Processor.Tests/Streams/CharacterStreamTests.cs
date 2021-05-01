using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class CharacterStreamTests
	{
		[Test]
		public async Task ReturnsCharsFromStream()
		{
			var charArray = new[] { 'a', 'b', 'c' };
			var stream = createStreamFrom(charArray);
			await using var characterStream = new CharacterStream(stream);

			var charsRead = new List<char>();
			char? charRead;
			while ((charRead = await characterStream.Read()) != null)
				charsRead.Add(charRead.Value);

			CollectionAssert.AreEqual(charArray, charsRead);
		}

		[Test]
		public async Task StreamDisposed_ReturnsNull()
		{
			var charArray = new[] { 'a' };
			var stream = createStreamFrom(charArray);
			await using var characterStream = new CharacterStream(stream);

			await characterStream.DisposeAsync();
			var nullResult = await characterStream.Read();

			Assert.Null(nullResult);
		}

		private Stream createStreamFrom(IEnumerable<char> chars)
		{
			return new MemoryStream(chars.Select(_ => (byte) _).ToArray());
		}
	}
}