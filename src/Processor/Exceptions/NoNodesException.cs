using System;

namespace YamlConfiguration.Processor
{
	public class NoNodesException : Exception
	{
		public NoNodesException(string message) : base(message) {}
	}
}