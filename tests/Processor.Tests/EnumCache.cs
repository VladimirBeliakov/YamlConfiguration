using System;
using System.Collections.Generic;
using System.Linq;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	internal static class EnumCache
	{
		public static IEnumerable<Context> GetBlockAndFlowTypes()
		{
			return Enum.GetValues(typeof(Context)).Cast<Context>().Except(getKeys());
		}

		public static IEnumerable<Context> GetBlockTypes()
		{
			return GetBlockAndFlowTypes().Except(GetFlowTypes());
		}

		public static IEnumerable<Context> GetFlowTypes()
		{
			yield return Context.FlowIn;
			yield return Context.FlowOut;
		}

		private static IEnumerable<Context> getKeys()
		{
			yield return Context.BlockKey;
			yield return Context.FlowKey;
		}
	}
}