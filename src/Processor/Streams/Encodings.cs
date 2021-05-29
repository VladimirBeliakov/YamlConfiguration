using System.Text;

namespace YamlConfiguration.Processor
{
	internal static class Encodings
	{
		public static readonly Encoding UTF8 = Encoding.UTF8;

		public static readonly Encoding BigEndianUTF16 = Encoding.BigEndianUnicode;

		public static readonly Encoding LittleEndianUTF16 = Encoding.Unicode;

		public static readonly Encoding BigEndianUTF32 = new UTF32Encoding(bigEndian: true, byteOrderMark: true);

		public static readonly Encoding LittleEndianUTF32 = Encoding.UTF32;
	}
}