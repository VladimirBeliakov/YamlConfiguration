using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	internal class YamlDirectiveParserTests : DirectiveParserBaseTests<YamlDirectiveParser, YamlDirective>
	{
		protected override string DirectiveName => "YAML";

		[TestCase("1.1.1")]
		[TestCase("a.1.1")]
		public async Task Process_InvalidYamlVersion_ReturnsNull(string yamlVersion)
		{
			var stream = CreateStream(additionalChars: yamlVersion);

			var result = await Process(stream);

			Assert.Null(result);
		}

		[TestCase("a")]
		[TestCase("-1")]
		[TestCase("0")]
		[TestCase("2")]
		public async Task Process_InvalidYamlMajorVersion_ReturnsNull(string yamlMajorVersion)
		{
			var stream = CreateStream(additionalChars: $"{yamlMajorVersion}.0");

			var result = await Process(stream);

			Assert.Null(result);
		}

		[TestCase("a")]
		[TestCase("-1")]
		public async Task Process_InvalidYamlMinorVersion_Returns(string yamlMinorVersion)
		{
			var stream = CreateStream(additionalChars: $"1.{yamlMinorVersion}");

			var result = await Process(stream);

			Assert.Null(result);
		}

		[TestCase("3")]
		[TestCase("4")]
		public async Task Process_YamlMinorVersionGreaterThan2_ReturnsMinorVersionChangedTo2(string yamlMinorVersion)
		{
			var stream = CreateStream(additionalChars: $"1.{yamlMinorVersion}");

			var result = await Process(stream);

			Assert.That(result!.YamlVersion.Minor, Is.EqualTo(2));
		}

		[TestCase(1, 0)]
		[TestCase(1, 1)]
		[TestCase(1, 2)]
		public async Task Process_ValidYamlVersion_ReturnsParsedYamlVersion(int major, int minor)
		{
			var stream = CreateStream(additionalChars: $"{major}.{minor}");

			var result = await Process(stream);

			Assert.That(result!.YamlVersion, Is.EqualTo(new Version(major, minor)));
		}
	}
}