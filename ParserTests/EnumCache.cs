using System;
using System.Collections.Generic;
using System.Linq;
using Parser.TypeDefinitions;

namespace ParserTests
{
	internal static class EnumCache
	{
		public static IEnumerable<BlockFlow> GetBlockAndFlowTypes()
		{
			return Enum.GetValues(typeof(BlockFlow)).Cast<BlockFlow>();
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
	}
}