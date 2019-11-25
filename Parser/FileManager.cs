using System;
using System.IO;
using System.Threading.Tasks;

namespace Parser
{
	public class FileManager
	{
		public static Task<FileStream> FindFileForReadOnly(string path)
		{
			return Task.Run(() => File.OpenRead(path));
		}
	}
}