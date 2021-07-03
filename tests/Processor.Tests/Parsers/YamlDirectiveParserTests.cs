using System;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using Sandbox;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class YamlDirectiveParserTests : DirectiveParserBaseTests
	{
		[TestCase("1.1.1")]
		[TestCase("a.1.1")]
		public async Task Process_InvalidYamlVersion_ReturnsNull(string yamlVersion)
		{
			var stream = CreateStream(directiveName: "YAML", additionalChars: yamlVersion);
			var directiveParser = createDirectiveParser();

			var result = await directiveParser.Process(stream);

			Assert.Null(result);
		}

		[TestCase("a")]
		[TestCase("-1")]
		[TestCase("0")]
		[TestCase("2")]
		public async Task Process_InvalidYamlMajorVersion_ReturnsNull(string yamlMajorVersion)
		{
			var stream = CreateStream(directiveName: "YAML", additionalChars: $"{yamlMajorVersion}.0");
			var directiveParser = createDirectiveParser();

			var result = await directiveParser.Process(stream);

			Assert.Null(result);
		}

		[TestCase("a")]
		[TestCase("-1")]
		public async Task Process_InvalidYamlMinorVersion_Returns(string yamlMinorVersion)
		{
			var stream = CreateStream(directiveName: "YAML", additionalChars: $"1.{yamlMinorVersion}");
			var directiveParser = createDirectiveParser();

			var result = await directiveParser.Process(stream);

			Assert.Null(result);
		}

		[TestCase("3")]
		[TestCase("4")]
		public async Task Process_YamlMinorVersionGreaterThan2_ReturnsMinorVersionChangedTo2(string yamlMinorVersion)
		{
			var stream = CreateStream(directiveName: "YAML", additionalChars: $"1.{yamlMinorVersion}");
			var directiveParser = createDirectiveParser();

			var result = (YamlDirective?) await directiveParser.Process(stream);

			Assert.That(result!.YamlVersion.Minor, Is.EqualTo(2));
		}

		[TestCase(1, 0)]
		[TestCase(1, 1)]
		[TestCase(1, 2)]
		public async Task Process_ValidYamlVersion_ReturnsParsedYamlVersion(int major, int minor)
		{
			var stream = CreateStream(directiveName: "YAML", additionalChars: $"{major}.{minor}");
			var directiveParser = createDirectiveParser();

			var result = (YamlDirective?) await directiveParser.Process(stream);

			Assert.That(result!.YamlVersion, Is.EqualTo(new Version(major, minor)));
		}

		private static YamlDirectiveParser createDirectiveParser() => new(A.Dummy<IOneLineCommentParser>());
	}
}