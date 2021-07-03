using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class TagDirective : IDirective
	{
		public TagDirective(string handle, string prefix)
		{
			Handle = handle;
			Prefix = prefix;
		}

		public Directive Type => Directive.Tag;
		
		public string Handle { get; }

		public string Prefix { get; }
	}
}