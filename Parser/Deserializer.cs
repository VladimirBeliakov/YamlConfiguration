using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Parser
{
	public class Deserializer
	{
		public async Task<T> Deserialize<T>(string value) where T : class
		{
			var stringLines = getStringLines(value);

			await foreach (var stringLine in stringLines)
			{
				Assembly.GetExecutingAssembly().DefinedTypes.Where(t =>
				{
					var dataContractAttributes = t.GetCustomAttributes().Where(a =>
					{
						if (!(a is DataContractAttribute dataContractAttribute))
							return false;

						return dataContractAttribute.Name == stringLine;
					});

					return t.IsClass && dataContractAttributes.Any();
				}).ToList();
			}
			
			return null;
		}

		private static async IAsyncEnumerable<string> getStringLines(string value)
		{
			var stringReader = new StringReader(value);
			yield return await stringReader.ReadLineAsync();
		}
	}
}