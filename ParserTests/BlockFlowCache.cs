using System;
using System.Collections.Generic;
using System.Linq;
using Parser.TypeDefinitions;

namespace ParserTests
{
	internal static class BlockFlowCache
	{
		public static IEnumerable<BlockFlowInOut> GetBlockAndFlowTypes()
		{
			return Enum.GetValues(typeof(BlockFlowInOut)).Cast<BlockFlowInOut>();
		}

		public static IEnumerable<BlockFlowInOut> GetBlockTypes()
		{
			return GetBlockAndFlowTypes().Except(GetFlowTypes());
		}

		public static IEnumerable<BlockFlowInOut> GetFlowTypes()
		{
			yield return BlockFlowInOut.FlowIn;
			yield return BlockFlowInOut.FlowOut;
		}
	}
}