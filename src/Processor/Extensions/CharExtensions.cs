namespace YamlConfiguration.Processor
{
	internal static class CharExtensions
	{
		public static bool IsWhiteSpace(this char @char) => @char == Characters.Space || @char == Characters.Tab;
	}
}