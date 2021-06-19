using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class ReservedDirective : IDirective
	{
		public Directive Type => Directive.Reserved;
	}
}