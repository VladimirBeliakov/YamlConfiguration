using System.Threading.Tasks;
using NUnit.Framework;
using Parser;

namespace DeserializerTests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public async Task Test1()
		{
			const string path = "..\\..\\..\\..\\TestConfigs\\TestConfig.yaml";
			
			var fileStream = await FileManager.OpenFileForReadOnly(path);

			using var fileStreamReader = new FileStreamReader(fileStream);
			var stringValue = await fileStreamReader.ReadString();


			var deserializer = new Deserializer();

			var deserializedResult = deserializer.Deserialize<object>(stringValue);
		}
	}
}