using System;

namespace Processor.Exceptions
{
	public class InvalidYamlMappingException : Exception
	{
		public InvalidYamlMappingException(string message) : base(message)
		{
		}
	}
}