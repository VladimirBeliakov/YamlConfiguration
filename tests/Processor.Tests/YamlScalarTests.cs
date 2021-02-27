using System;
using NUnit.Framework;
using YamlConfiguration.Processor.Exceptions;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class YamlScalarTests
	{
		[Test]
		public void Add_ValidItem_Success(
			[Values("", " # comment")] string comment, 
			[Values] bool isCollectionItem
		)
		{
			var item = isCollectionItem ? "  - type_Name1" : "- type_Name1";
			var @break = Environment.NewLine;
			Assert.DoesNotThrow(() => new YamlScalar(item + comment + @break, isCollectionItem));
		}

		[TestCase(" type_Name1", "")]
		[TestCase("-type_Name1", "")]
		[TestCase("type_Name1", "")]
		[TestCase("- type Name1", "")]
		[TestCase("- !@#$%^&*()-=+", "")]
		[TestCase("- type_Name1", " invalid comment")]
		public void Add_InvalidItem_Throws(string item, string comment)
		{
			var @break = Environment.NewLine;
			Assert.Throws<InvalidYamlCollectionItemException>(() => new YamlScalar(item + comment + @break));
		}
	}
}