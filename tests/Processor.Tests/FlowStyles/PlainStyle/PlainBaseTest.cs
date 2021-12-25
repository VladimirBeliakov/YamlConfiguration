using System;
using System.Collections.Generic;
using System.Linq;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	public abstract class PlainBaseTest
	{
		protected static IEnumerable<IReadOnlyCollection<string>> GetNsPlainSafeCharGroups(Context context)
		{
			var excludedChars = context switch
			{
				Context.FlowOut or Context.BlockKey => Enumerable.Empty<string>(),
				Context.FlowIn or Context.FlowKey => CharStore.FlowIndicators,
				_ => throw new ArgumentOutOfRangeException(
						nameof(context),
						context,
						$"Only {Context.BlockKey}, {Context.FlowKey}, " +
						$"{Context.FlowIn} and {Context.FlowOut} are supported."
					)
			};

			var nsPlainSafeCharsWithoutSurrogates =
				CharStore.GetNsCharsWithoutSurrogates().Except(excludedChars).ToList();

			yield return nsPlainSafeCharsWithoutSurrogates;

			var nsPlainSafeSurrogates = CharStore.SurrogatePairs.Value;

			yield return nsPlainSafeSurrogates;
		}
	}
}
