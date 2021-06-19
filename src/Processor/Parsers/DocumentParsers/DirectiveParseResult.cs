using System.Collections.Generic;

namespace YamlConfiguration.Processor
{
	internal class DirectiveParseResult
	{
		public DirectiveParseResult(IReadOnlyCollection<IDirective> directives, bool isDirectiveEndPresent = false)
		{
			Directives = directives;
			IsDirectiveEndPresent = isDirectiveEndPresent;
		}

		public IReadOnlyCollection<IDirective> Directives { get; }
		
		public bool IsDirectiveEndPresent { get; }

		public void Deconstruct(out IReadOnlyCollection<IDirective> directives, out bool isDirectiveEndPresent)
		{
			directives = Directives;
			isDirectiveEndPresent = IsDirectiveEndPresent;
		}
	}
}