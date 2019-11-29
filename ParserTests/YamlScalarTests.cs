using NUnit.Framework;
using NUnit.Framework.Constraints;
using Parser.Exceptions;
using Parser.TypeDefinitions;

namespace DeserializerTests
{
	[TestFixture, Parallelizable(ParallelScope.Children)]
	public class YamlScalarTests
	{
		[Test]
		public void Add_ValidItem_Success(
			[Values("", " # comment")] string comment, 
			[Values] bool isCollectionItem
		)
		{
			var item = isCollectionItem ? "  - type_Name1" : "- type_Name1";
			Assert.DoesNotThrow(() => new YamlScalar(item + comment, isCollectionItem));
		}

		[TestCase(" type_Name1", "")]
		[TestCase("-type_Name1", "")]
		[TestCase("type_Name1", "")]
		[TestCase("- type Name1", "")]
		[TestCase("- !@#$%^&*()-=+", "")]
		[TestCase("- type_Name1 ", "")]
		[TestCase("- type_Name1", " invalid comment")]
		public void Add_InvalidItem_Throws(string item, string comment)
		{
			Assert.Throws<InvalidYamlCollectionItemException>(() => new YamlScalar(item + comment));
		}
	}
}