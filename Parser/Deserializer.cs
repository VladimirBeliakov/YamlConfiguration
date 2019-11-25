using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
	public class Deserializer
	{
		public async Task<T> Deserialize<T>(Stream stream) where T : class
		{
			var buffer = new byte[4096];

			await stream.ReadAsync(buffer, 0, buffer.Length);

			var stringResult = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
			
			return null;
		}
	}
}