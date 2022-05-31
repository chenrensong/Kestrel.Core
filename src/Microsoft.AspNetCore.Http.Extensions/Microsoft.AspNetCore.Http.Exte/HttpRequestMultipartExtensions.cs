using System;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http.Extensions;

public static class HttpRequestMultipartExtensions
{
	public static string GetMultipartBoundary(this HttpRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var parsedValue))
		{
			return string.Empty;
		}
		return HeaderUtilities.RemoveQuotes(parsedValue.Boundary).ToString();
	}
}
