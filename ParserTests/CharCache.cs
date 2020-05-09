using System;
using System.Linq;
using Parser;

namespace ParserTests
{
	internal static class CharCache
	{
		public static string Spaces = String.Join(
			String.Empty,
			Enumerable.Repeat(' ', GlobalConstants.CharSequenceLength)
		);

		public static string Tabs = String.Join(
			String.Empty,
			Enumerable.Repeat('\t', GlobalConstants.CharSequenceLength)
		);

		public static string SpacesAndTabs = String.Join(
			String.Empty,
			Enumerable.Repeat("\t ", GlobalConstants.CharSequenceLength / 2)
		);
	}
}