using NUnit.Framework;
using YamlConfiguration.Processor.Exceptions;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class YamlMappingTests
	{
		[TestCase("key_1:  65", "")]
		[TestCase("key_1:  65", " # comment")]
		public void Constructor_ValidKeyValuePair_Success(string keyValuePair, string comment)
		{
			Assert.DoesNotThrow(() => { new YamlMapping(keyValuePair + comment); });
		}

		[TestCase(":  65", "")]
		[TestCase("!@#$%^&*()-+=:  65", "")]
		[TestCase("key_1:65", "")]
		[TestCase("key_1:", "")]
		// TODO: Reconsider using these cases after making the mind about what format the value can be.
//		[TestCase("key_1:  65", "# comment")]
//		[TestCase("key_1:  65", " invalid comment")]
		public void Constructor_InvalidKeyValuePair_Throws(string keyValuePair, string comment)
		{
			Assert.Throws<InvalidYamlMappingException>(() => { new YamlMapping(keyValuePair + comment); });
		}
	}
}