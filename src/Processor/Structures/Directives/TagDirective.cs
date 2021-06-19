using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class TagDirective : IDirective
	{
		public Directive Type => Directive.Tag;
	}
}