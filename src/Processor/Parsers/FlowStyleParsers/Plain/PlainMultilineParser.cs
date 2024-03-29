using System;
using System.Text;
using System.Threading.Tasks;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor
{
	internal class PlainMultilineParser : IPlainMultilineParser
	{
		private readonly IPlainInOneLineParser _plainInOneLineParser;
		private readonly IPlainNextLineParser _plainNextLineParser;
		private readonly IFlowFoldedLinesParser _flowFoldedLinesParser;

		private readonly char _breakChar = BasicStructures.Break;
		private readonly char _spaceChar = Characters.Space;

		public PlainMultilineParser(
			IPlainInOneLineParser plainOneLineParser,
			IPlainNextLineParser plainNextLineParser,
			IFlowFoldedLinesParser flowFoldedLinesParser
		)
		{
			_flowFoldedLinesParser = flowFoldedLinesParser;
			_plainInOneLineParser = plainOneLineParser;
			_plainNextLineParser = plainNextLineParser;
		}

		public async ValueTask<PlainLineNode?> TryProcess(
			ICharacterStream charStream,
			uint indentLength,
			Context context
		)
		{
			if (context is not Context.FlowIn and not Context.FlowOut)
				throw new ArgumentException(
					$"Only {Context.FlowIn} and {Context.FlowOut} are allowed.", nameof(context)
				);

			var firstLine = await _plainInOneLineParser.TryProcess(charStream, context).ConfigureAwait(false);

			if (firstLine is null)
				return null;

			var result = new StringBuilder();

			result.Append(firstLine.Value);

			while (true)
			{
				// If the next line is empty, we just discard any folded lines 
				// since they are not important for the next node.
				var flowFoldedLinesResult =
					await _flowFoldedLinesParser.Process(charStream, indentLength).ConfigureAwait(false);

				var nextLine =
					await _plainNextLineParser.TryProcess(charStream, context).ConfigureAwait(false);

				if (String.IsNullOrEmpty(nextLine))
					break;

				if (flowFoldedLinesResult.FoldedLineResult is not null)
					if (flowFoldedLinesResult.FoldedLineResult.IsBreakAsSpace)
						result.Append(_spaceChar);
					else
						for (var i = 0; i < flowFoldedLinesResult.FoldedLineResult.EmptyLineCount; i++)
							result.Append(_breakChar);
				else
					throw new InvalidYamlException("Plain lines must be separated by folded lines.");

				result.Append(nextLine);
			}

			return new PlainLineNode(result.ToString());
		}
	}
}
