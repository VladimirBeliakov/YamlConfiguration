using System;

namespace YamlConfiguration.Processor.Exceptions
{
	public class InvalidYamlCollectionItemException : Exception
	{
		public InvalidYamlCollectionItemException(string message) : base(message)
		{
		}
	}
}