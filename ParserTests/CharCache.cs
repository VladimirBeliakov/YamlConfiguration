using System;
using System.Collections.Generic;
using System.Linq;
using Parser;

namespace ParserTests
{
	internal static class CharCache
	{
		public static string Spaces = String.Join(
			String.Empty,
			Enumerable.Repeat(' ', GlobalConstants.CharGroupLength)
		);

		public static string Tabs = String.Join(
			String.Empty,
			Enumerable.Repeat('\t', GlobalConstants.CharGroupLength)
		);

		public static string SpacesAndTabs = String.Join(
			String.Empty,
			Enumerable.Repeat("\t ", GlobalConstants.CharGroupLength / 2)
		);

		public static string Chars = String.Join(
			String.Empty,
			Enumerable.Repeat("ABCD", GlobalConstants.CharGroupLength / 4)
		);

		public static IEnumerable<string> SeparateInLineCases = new[] { " ", "\t", Spaces, Tabs, SpacesAndTabs };
	}
}