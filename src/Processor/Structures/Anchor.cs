namespace YamlConfiguration.Processor
{
	internal class Anchor
	{
		public Anchor(string name, INode value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; set; }

		public INode Value { get; set; }
	}
}