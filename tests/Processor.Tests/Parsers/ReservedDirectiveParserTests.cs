using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	internal class ReservedDirectiveParserTests : DirectiveParserBaseTests<ReservedDirectiveParser, ReservedDirective>
	{
		protected override string DirectiveName => "Name";

		[Test]
		public async Task Parse_InvalidDirective_ReturnsNull()
		{
			var stream = CreateStream(String.Empty);

			var result = await Process(stream);

			Assert.Null(result);
		}

		[Test]
		public async Task Parse_ValidDirective_ReturnsDirective()
		{
			var name = DirectiveName;
			const string parameter = "Parameter";
			var stream = CreateStream($"{parameter}");

			var result = await Process(stream);

			Assert.Multiple(() =>
				{
					Assert.That(result!.Name, Is.EqualTo(name));
					Assert.That(result.Parameter, Is.EqualTo(parameter));
				}
			);
		}
	}
}