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
			Enumerable.Repeat(' ', Characters.CharGroupLength)
		);

		public static string Tabs = String.Join(
			String.Empty,
			Enumerable.Repeat('\t', Characters.CharGroupLength)
		);

		public static string SpacesAndTabs = String.Join(
			String.Empty,
			Enumerable.Repeat("\t ", Characters.CharGroupLength / 2)
		);

		public static string Chars = String.Join(
			String.Empty,
			Enumerable.Repeat("ABCD", Characters.CharGroupLength / 4)
		);

		public static string Digits = String.Join(
			String.Empty,
			Enumerable.Repeat("0123456789", Characters.CharGroupLength / 10)
		);

		public static IEnumerable<string> SeparateInLineCases = new[] { " ", "\t", Spaces, Tabs, SpacesAndTabs };
	}
}