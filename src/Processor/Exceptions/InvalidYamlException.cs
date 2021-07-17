using System;

namespace YamlConfiguration.Processor
{
	public class InvalidYamlException : Exception
	{
		public InvalidYamlException(string message) : base(message) {}
	}
}