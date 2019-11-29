using System;

namespace Parser.Exceptions
{
	public class InvalidYamlMappingException : Exception
	{
		public InvalidYamlMappingException(string message) : base(message)
		{
		}
	}
}