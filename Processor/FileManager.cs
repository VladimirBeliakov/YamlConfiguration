using System;
using System.IO;
using System.Threading.Tasks;

namespace Processor
{
	public class FileManager
	{
		public static Task<FileStream> OpenFileForReadOnly(string path)
		{
			return Task.Run(() => File.OpenRead(path));
		}
	}
}