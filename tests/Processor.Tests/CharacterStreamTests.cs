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
			await using var characterStream = new CharacterStream(stream);

			var charsRead = new List<char>();
			char? charRead;
			while ((charRead = await characterStream.Read()) != null)
				charsRead.Add(charRead.Value);

			CollectionAssert.AreEqual(charArray, charsRead);
		}
	}
}