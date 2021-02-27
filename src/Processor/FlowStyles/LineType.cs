namespace YamlConfiguration.Processor.FlowStyles
{
	public enum LineType : byte
	{
		First = 1,
		NotEmpty = 2,
		Empty = 3,
		LastNotEmpty = 4,
		LastEmpty = 5,
		Invalid = 6,
	}
}