using System;
using System.Collections.Generic;
using System.Linq;
using YamlConfiguration.Processor.TypeDefinitions;

namespace YamlConfiguration.Processor.Tests
{
	internal static class EnumCache
	{
		public static IEnumerable<BlockFlow> GetBlockAndFlowTypes()
		{
			return Enum.GetValues(typeof(BlockFlow)).Cast<BlockFlow>().Except(getKeys());
		}

		public static IEnumerable<BlockFlow> GetBlockTypes()
		{
			return GetBlockAndFlowTypes().Except(GetFlowTypes());
		}

		public static IEnumerable<BlockFlow> GetFlowTypes()
		{
			yield return BlockFlow.FlowIn;
			yield return BlockFlow.FlowOut;
		}

		private static IEnumerable<BlockFlow> getKeys()
		{
			yield return BlockFlow.BlockKey;
			yield return BlockFlow.FlowKey;
		}
	}
}