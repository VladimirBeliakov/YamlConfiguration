using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessorTests.Extensions
{
	internal static class StringExtensions
	{
		public static IEnumerable<string> GroupBy(this IEnumerable<string> values, int valuesInGroup)
		{
			if (valuesInGroup < 1)
				throw new InvalidOperationException($"{nameof(valuesInGroup)} must be greater than zero.");

			var firstValueLength = values.First().Length;

			if (values.Any(v => v.Length != firstValueLength))
				throw new InvalidOperationException(
					$"All value lengths of {nameof(values)} must be equal to each other"
				);

			var oneGroupLength = firstValueLength * valuesInGroup;

			var sb = new StringBuilder(oneGroupLength);

			foreach (var value in values)
			{
				sb.Append(value);

				if (sb.Length == oneGroupLength)
				{
					yield return sb.ToString();
					sb.Clear();
				}
			}

			if (sb.Length > 0)
				yield return sb.ToString();
		}
	}
}