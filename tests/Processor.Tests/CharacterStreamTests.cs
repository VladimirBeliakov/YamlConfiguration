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
		public async Task PeeksCharsFromStream()
		{
			var charArray = new[] { 'a', 'b', 'c' };
			var stream = createStreamFrom(charArray);
			await using var characterStream = new CharacterStream(stream);

			var char1 = await characterStream.Peek();
			await characterStream.Read();
			var char2 = await characterStream.Peek();
			await characterStream.Read();
			var char3 = await characterStream.Peek();
			await characterStream.Read();

			CollectionAssert.AreEqual(charArray, new[] { char1, char2, char3 });
		}

		[Test]
		public async Task PeekingDoesNotCauseStreamToAdvance()
		{
			var charArray = new[] { 'a', 'b' };
			var stream = createStreamFrom(charArray);
			await using var characterStream = new CharacterStream(stream);

			var char1 = await characterStream.Peek();
			var char2 = await characterStream.Peek();

			Assert.That('a', Is.EqualTo(char1).And.EqualTo(char2));
		}

		private Stream createStreamFrom(IEnumerable<char> chars)
		{
			return new MemoryStream(chars.Select(_ => (byte) _).ToArray());
		}
	}
}