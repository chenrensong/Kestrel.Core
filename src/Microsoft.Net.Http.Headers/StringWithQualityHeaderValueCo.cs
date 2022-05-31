using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class StringWithQualityHeaderValueComparer : IComparer<StringWithQualityHeaderValue>
{
	private static readonly StringWithQualityHeaderValueComparer _qualityComparer = new StringWithQualityHeaderValueComparer();

	public static StringWithQualityHeaderValueComparer QualityComparer => _qualityComparer;

	private StringWithQualityHeaderValueComparer()
	{
	}

	public int Compare(StringWithQualityHeaderValue stringWithQuality1, StringWithQualityHeaderValue stringWithQuality2)
	{
		if (stringWithQuality1 == null)
		{
			throw new ArgumentNullException("stringWithQuality1");
		}
		if (stringWithQuality2 == null)
		{
			throw new ArgumentNullException("stringWithQuality2");
		}
		double num = stringWithQuality1.Quality ?? 1.0;
		double num2 = stringWithQuality2.Quality ?? 1.0;
		double num3 = num - num2;
		if (num3 < 0.0)
		{
			return -1;
		}
		if (num3 > 0.0)
		{
			return 1;
		}
		if (!StringSegment.Equals(stringWithQuality1.Value, stringWithQuality2.Value, StringComparison.OrdinalIgnoreCase))
		{
			if (StringSegment.Equals(stringWithQuality1.Value, "*", StringComparison.Ordinal))
			{
				return -1;
			}
			if (StringSegment.Equals(stringWithQuality2.Value, "*", StringComparison.Ordinal))
			{
				return 1;
			}
		}
		return 0;
	}
}
