using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	internal class TagDirectiveParserTests : DirectiveParserBaseTests<TagDirectiveParser, TagDirective>
	{
		protected override string DirectiveName => "TAG";

		[Test]
		public async Task Parse_InvalidTag_ReturnsNull()
		{
			var stream = CreateStream(additionalChars: "invalid_handle! prefix");

			var result = await Process(stream);

			Assert.Null(result);
		}

		[Test]
		public async Task Parse_ValidTag_ReturnsParsedTagWithHandleAndPrefix()
		{
			const string handle = "!handle!";
			const string prefix = "prefix";
			var stream = CreateStream(additionalChars: $"{handle} {prefix}");

			var result = await Process(stream);

			Assert.Multiple(() =>
				{
					Assert.That(result?.Handle, Is.EqualTo(handle));
					Assert.That(result?.Prefix, Is.EqualTo(prefix));
				}
			);
		}
	}
}