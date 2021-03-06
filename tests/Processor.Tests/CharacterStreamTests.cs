using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	public class CharacterStreamTests
	{
		[Test]
		public async Task ReturnsCharsFromStream()
		{
			var charArray = new[] { 'a', 'b', 'c' };
			var stream = new MemoryStream(charArray.Select(_ => (byte) _).ToArray());
			var characterStream = new CharacterStream(stream);

			var charsRead = new List<char>();
			await foreach (var charRead in characterStream)
				charsRead.Add(charRead);

			CollectionAssert.AreEqual(charArray, charsRead);
		}
	}
}