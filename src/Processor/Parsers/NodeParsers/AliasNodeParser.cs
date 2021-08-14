using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlConfiguration.Processor.FlowStyles;

namespace YamlConfiguration.Processor
{
	internal class AliasNodeParser : IAliasNodeParser
	{
		private static readonly Regex _aliasNodeRegex = new(SimpleStyles.AliasNode, RegexOptions.Compiled);

		public async ValueTask<INode?> Process(ICharacterStream charStream)
		{
			var possibleAliasChar = await charStream.Peek().ConfigureAwait(false);

			if (possibleAliasChar != Characters.Alias)
				return null;

			var readLine = await charStream.ReadLine().ConfigureAwait(false);

			var match = _aliasNodeRegex.Match(readLine);

			if (!match.Success)
				throw new InvalidYamlException($"Invalid alias {readLine}.");

			var aliasName = match.Groups[1].Captures[0].Value;

			return new AliasNode(aliasName);
		}
	}
}