using System;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public static class ContentDispositionHeaderValueIdentityExtensions
{
	public static bool IsFileDisposition(this ContentDispositionHeaderValue header)
	{
		if (header == null)
		{
			throw new ArgumentNullException("header");
		}
		if (header.DispositionType.Equals("form-data"))
		{
			if (StringSegment.IsNullOrEmpty(header.FileName))
			{
				return !StringSegment.IsNullOrEmpty(header.FileNameStar);
			}
			return true;
		}
		return false;
	}

	public static bool IsFormDisposition(this ContentDispositionHeaderValue header)
	{
		if (header == null)
		{
			throw new ArgumentNullException("header");
		}
		if (header.DispositionType.Equals("form-data") && StringSegment.IsNullOrEmpty(header.FileName))
		{
			return StringSegment.IsNullOrEmpty(header.FileNameStar);
		}
		return false;
	}
}
