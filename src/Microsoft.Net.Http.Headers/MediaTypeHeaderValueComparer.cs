using System;
using System.Collections.Generic;

namespace Microsoft.Net.Http.Headers;

public class MediaTypeHeaderValueComparer : IComparer<MediaTypeHeaderValue>
{
	private static readonly MediaTypeHeaderValueComparer _mediaTypeComparer = new MediaTypeHeaderValueComparer();

	public static MediaTypeHeaderValueComparer QualityComparer => _mediaTypeComparer;

	private MediaTypeHeaderValueComparer()
	{
	}

	public int Compare(MediaTypeHeaderValue mediaType1, MediaTypeHeaderValue mediaType2)
	{
		if (mediaType1 == mediaType2)
		{
			return 0;
		}
		int num = CompareBasedOnQualityFactor(mediaType1, mediaType2);
		if (num == 0)
		{
			if (!mediaType1.Type.Equals(mediaType2.Type, StringComparison.OrdinalIgnoreCase))
			{
				if (mediaType1.MatchesAllTypes)
				{
					return -1;
				}
				if (mediaType2.MatchesAllTypes)
				{
					return 1;
				}
				if (mediaType1.MatchesAllSubTypes && !mediaType2.MatchesAllSubTypes)
				{
					return -1;
				}
				if (!mediaType1.MatchesAllSubTypes && mediaType2.MatchesAllSubTypes)
				{
					return 1;
				}
				if (mediaType1.MatchesAllSubTypesWithoutSuffix && !mediaType2.MatchesAllSubTypesWithoutSuffix)
				{
					return -1;
				}
				if (!mediaType1.MatchesAllSubTypesWithoutSuffix && mediaType2.MatchesAllSubTypesWithoutSuffix)
				{
					return 1;
				}
			}
			else if (!mediaType1.SubType.Equals(mediaType2.SubType, StringComparison.OrdinalIgnoreCase))
			{
				if (mediaType1.MatchesAllSubTypes)
				{
					return -1;
				}
				if (mediaType2.MatchesAllSubTypes)
				{
					return 1;
				}
				if (mediaType1.MatchesAllSubTypesWithoutSuffix && !mediaType2.MatchesAllSubTypesWithoutSuffix)
				{
					return -1;
				}
				if (!mediaType1.MatchesAllSubTypesWithoutSuffix && mediaType2.MatchesAllSubTypesWithoutSuffix)
				{
					return 1;
				}
			}
			else if (!mediaType1.Suffix.Equals(mediaType2.Suffix, StringComparison.OrdinalIgnoreCase))
			{
				if (mediaType1.MatchesAllSubTypesWithoutSuffix)
				{
					return -1;
				}
				if (mediaType2.MatchesAllSubTypesWithoutSuffix)
				{
					return 1;
				}
			}
		}
		return num;
	}

	private static int CompareBasedOnQualityFactor(MediaTypeHeaderValue mediaType1, MediaTypeHeaderValue mediaType2)
	{
		double num = mediaType1.Quality ?? 1.0;
		double num2 = mediaType2.Quality ?? 1.0;
		double num3 = num - num2;
		if (num3 < 0.0)
		{
			return -1;
		}
		if (num3 > 0.0)
		{
			return 1;
		}
		return 0;
	}
}
