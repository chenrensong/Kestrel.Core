using System;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http;

public static class ResponseExtensions
{
	public static void Clear(this HttpResponse response)
	{
		if (response.HasStarted)
		{
			throw new InvalidOperationException("The response cannot be cleared, it has already started sending.");
		}
		response.StatusCode = 200;
		response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = null;
		response.Headers.Clear();
		if (response.Body.CanSeek)
		{
			response.Body.SetLength(0L);
		}
	}
}
