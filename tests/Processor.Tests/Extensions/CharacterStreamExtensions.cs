using FakeItEasy;

namespace YamlConfiguration.Processor.Tests
{
	internal static class CharacterStreamExtensions
	{
		public static void AssertNotAdvanced(this ICharacterStream stream)
		{
			A.CallTo(() => stream.Read()).MustNotHaveHappened();
			A.CallTo(() => stream.Read(A<int>._)).MustNotHaveHappened();
		}
	}
}