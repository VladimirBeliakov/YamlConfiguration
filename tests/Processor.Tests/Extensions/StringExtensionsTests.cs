using System;
using System.Linq;
using NUnit.Framework;

namespace Processor.Tests
{
	[TestFixture]
	public class StringExtensionsTests
	{
		[Test]
		public void GroupBy_ValuesCountLessThanValuesInGroup_ReturnsCorrectlyGroupedValues()
		{
			var result = new[] { "12", "34" }.GroupBy(3);

			CollectionAssert.AreEquivalent(new[] { "1234" }, result);
		}

		[Test]
		public void GroupBy_ValuesCountEqualsToValuesInGroup_ReturnsCorrectlyGroupedValues()
		{
			var result = new[] { "12", "34" }.GroupBy(2);

			CollectionAssert.AreEquivalent(new[] { "1234" }, result);
		}

		[Test]
		public void GroupBy_ValuesCountGreaterThanValuesInGroup_ReturnsCorrectlyGroupedValues()
		{
			var result = new[] { "12", "34" }.GroupBy(1);

			CollectionAssert.AreEquivalent(new[] { "12", "34" }, result);
		}

		[TestCase(0)]
		[TestCase(-1)]
		public void GroupBy_ValuesInGroupLessThanOne_Throws(int invalidValuesInGroup)
		{
			Assert.Throws<InvalidOperationException>(() => new[] { "test" }.GroupBy(invalidValuesInGroup).ToList());
		}

		[Test]
		public void GroupBy_DifferentValueLengths_Throws()
		{
			Assert.Throws<InvalidOperationException>(() => new[] { "1", "12" }.GroupBy(1).ToList());
		}
	}
}