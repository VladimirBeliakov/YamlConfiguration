using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace YamlConfiguration.Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	internal abstract class DirectiveParserBaseTests<TParser, TDirective>
		where TParser : OneDirectiveParser
		where TDirective : IDirective
	{
		protected abstract string DirectiveName { get; }

		protected ICharacterStream CreateStream(string additionalChars)
		{
			var stream = A.Fake<ICharacterStream>();

			const char directiveChar = '%';

			A.CallTo(() => stream.Peek()).Returns(directiveChar);

			var directiveCharAndName = new[] { directiveChar }.Concat(DirectiveName).ToArray();

			A.CallTo(() => stream.Peek(directiveCharAndName.Length)).Returns(directiveCharAndName);

			A.CallTo(() => stream.ReadLine())
				.Returns($"{directiveChar}{DirectiveName} {additionalChars}{Environment.NewLine}");

			return stream;
		}

		protected static async Task<TDirective?> Process(ICharacterStream charStream)
		{
			var parser = (TParser) Activator.CreateInstance(typeof(TParser), A.Dummy<IMultiLineCommentParser>())!;

			return (TDirective?) await parser.Process(charStream);
		}
	}
}