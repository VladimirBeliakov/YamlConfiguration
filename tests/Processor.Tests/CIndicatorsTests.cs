using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Processor;

namespace Processor.Tests
{
	[TestFixture, Parallelizable(ParallelScope.All)]
	public class CIndicatorsTests
	{
		[TestCaseSource(nameof(_cIndicators))]
		public void ValidCIndicator_Matches(string cIndicator)
		{
			var match = _regex.Match(cIndicator);
			Assert.True(match.Success);
		}

		[TestCase("a")]
		[TestCase(".")]
		[TestCase(")")]
		public void InvalidCIndicator_DoesNotMatch(string cIndicator)
		{
			var match = _regex.Match(cIndicator);
			Assert.False(match.Success);
		}

		private static IEnumerable<string> _cIndicators = CharStore.CIndicators;

		private readonly Regex _regex = new Regex("[" + Characters.CIndicators + "]");
	}
}