using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal interface IDirective
	{
		Directive Type { get; }
	}
}