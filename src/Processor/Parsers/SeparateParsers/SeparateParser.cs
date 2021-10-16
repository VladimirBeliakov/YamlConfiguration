using System;
using System.Threading.Tasks;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class SeparateParser
	{
		private readonly ISeparateLinesParser _separateLinesParser;
		private readonly ISeparateInLineParser _separateInLineParser;

		public SeparateParser(ISeparateLinesParser separateLinesParser, ISeparateInLineParser separateInLineParser)
		{
			_separateLinesParser = separateLinesParser;
			_separateInLineParser = separateInLineParser;
		}

		public ValueTask<bool> TryProcess(
			ICharacterStream charStream,
			Context context,
			uint? indentLength = null
		)
		{
			switch (context)
			{
				case Context.BlockIn:
				case Context.BlockOut:
				case Context.FlowIn:
				case Context.FlowOut:
					return _separateLinesParser.TryProcess(charStream, indentLength);
				case Context.BlockKey:
				case Context.FlowKey:
					return _separateInLineParser.TryProcess(charStream);
				default:
					throw new ArgumentOutOfRangeException(nameof(context), context, null);
			}
		}
	}
}