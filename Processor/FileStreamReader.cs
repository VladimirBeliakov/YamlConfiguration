using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Processor
{
	public class FileStreamReader : IDisposable
	{
		private readonly Stream _stream;

		public FileStreamReader(Stream stream)
		{
			_stream = stream;
		}
		
		public async Task<string> ReadString()
		{
			var sb = new StringBuilder((int) _stream.Length);
			
			var buffer = new byte[4096];
			var offset = 0;

			while (_stream.Position != _stream.Length)
			{
				await _stream.ReadAsync(buffer, offset, buffer.Length);
				sb.Append(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
				offset += buffer.Length;
			}

			return sb.ToString();
		}

		public void Dispose()
		{
			_stream.Dispose();
		}
	}
}