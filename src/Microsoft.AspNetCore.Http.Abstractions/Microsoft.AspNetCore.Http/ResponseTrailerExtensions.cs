using System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http;

public static class ResponseTrailerExtensions
{
	private const string Trailer = "Trailer";

	public static void DeclareTrailer(this HttpResponse response, string trailerName)
	{
		response.Headers.AppendCommaSeparatedValues("Trailer", trailerName);
	}

	public static bool SupportsTrailers(this HttpResponse response)
	{
		IHttpResponseTrailersFeature httpResponseTrailersFeature = response.HttpContext.Features.Get<IHttpResponseTrailersFeature>();
		if (httpResponseTrailersFeature?.Trailers != null)
		{
			return !httpResponseTrailersFeature.Trailers.IsReadOnly;
		}
		return false;
	}

	public static void AppendTrailer(this HttpResponse response, string trailerName, StringValues trailerValues)
	{
		IHttpResponseTrailersFeature httpResponseTrailersFeature = response.HttpContext.Features.Get<IHttpResponseTrailersFeature>();
		if (httpResponseTrailersFeature?.Trailers == null || httpResponseTrailersFeature.Trailers.IsReadOnly)
		{
			throw new InvalidOperationException("Trailers are not supported for this response.");
		}
		httpResponseTrailersFeature.Trailers.Append(trailerName, trailerValues);
	}
}
