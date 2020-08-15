using System;
using System.Collections.Generic;
using System.Linq;
using Processor;

namespace ProcessorTests
{
	internal static class CharCache
	{
		public static string Spaces = getCharRange(" ");

		public static string Tabs = getCharRange("\t");

		public static string SpacesAndTabs = getCharRange("\t ");

		public static string Chars = getCharRange("ABCD");

		public static string Digits = getCharRange("0123456789");

		private static string getCharRange(string chars)
		{
			return String.Join(
				String.Empty,
				Enumerable.Repeat(chars, Characters.CharGroupLength / chars.Length)
			);
		}

		public static IReadOnlyCollection<string> SeparateInLineCases = new[] { " ", "\t", Spaces, Tabs, SpacesAndTabs };
	}
}