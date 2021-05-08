using System.Collections.Generic;

namespace YamlConfiguration.Processor
{
	internal class DirectiveParseResult
	{
		public DirectiveParseResult(IReadOnlyCollection<Directive> directives, bool isDirectiveEndPresent)
		{
			Directives = directives;
			IsDirectiveEndPresent = isDirectiveEndPresent;
		}

		public IReadOnlyCollection<Directive> Directives { get; }
		
		public bool IsDirectiveEndPresent { get; }

		public void Deconstruct(out IReadOnlyCollection<Directive> directives, out bool isDirectiveEndPresent)
		{
			directives = Directives;
			isDirectiveEndPresent = IsDirectiveEndPresent;
		}
	}
}