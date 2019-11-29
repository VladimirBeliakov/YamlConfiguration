using System;

namespace Parser.Exceptions
{
	public class InvalidYamlCollectionItemException : Exception
	{
		public InvalidYamlCollectionItemException(string message) : base(message)
		{
		}
	}
}