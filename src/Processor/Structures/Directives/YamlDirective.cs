using System;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class YamlDirective : IDirective
	{
		public YamlDirective(Version yamlVersion)
		{
			YamlVersion = yamlVersion;
		}

		public Directive Type => Directive.Yaml;

		public Version YamlVersion { get; }
	}
}