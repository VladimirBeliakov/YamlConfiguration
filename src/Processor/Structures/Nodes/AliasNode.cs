namespace YamlConfiguration.Processor
{
	public class AliasNode : INode
	{
		public AliasNode(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}