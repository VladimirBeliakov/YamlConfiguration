using System;
using System.Threading.Tasks;
using Parser;

namespace Program
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			const string path = "..\\..\\..\\..\\TestConfigs\\TestConfig.yaml";
			
			var fileStream = await FileManager.FindFileForReadOnly(path);

			using var streamReader = new StreamReader(fileStream);
			var stringValue = await streamReader.ReadString();
		}
	}
}