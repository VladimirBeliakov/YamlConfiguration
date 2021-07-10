using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class ReservedDirective : IDirective
	{
		public ReservedDirective(string name, string parameter)
		{
			Name = name;
			Parameter = parameter;
		}

		public Directive Type => Directive.Reserved;
		
		public string Name { get; }

		public string Parameter { get; }
	}
}