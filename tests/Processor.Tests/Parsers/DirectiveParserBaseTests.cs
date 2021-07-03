using System;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public abstract class DirectiveParserBaseTests
	{
		internal static ICharacterStream CreateStream(string directiveName, string additionalChars)
		{
			var stream = A.Fake<ICharacterStream>();

			const char directiveChar = '%';

			A.CallTo(() => stream.Peek()).Returns(directiveChar);

			var directiveCharAndName = new[] { directiveChar }.Concat(directiveName).ToArray();

			A.CallTo(() => stream.Peek(directiveCharAndName.Length)).Returns(directiveCharAndName);

			A.CallTo(() => stream.ReadLine())
				.Returns($"{directiveChar}{directiveName} {additionalChars}{Environment.NewLine}");

			return stream;
		}
	}
}