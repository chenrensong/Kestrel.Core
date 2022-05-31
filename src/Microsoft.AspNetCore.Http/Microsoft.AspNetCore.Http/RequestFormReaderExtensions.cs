using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http;

public static class RequestFormReaderExtensions
{
	public static Task<IFormCollection> ReadFormAsync(this HttpRequest request, FormOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (!request.HasFormContentType)
		{
			throw new InvalidOperationException("Incorrect Content-Type: " + request.ContentType);
		}
		IFeatureCollection features = request.HttpContext.Features;
		IFormFeature formFeature = features.Get<IFormFeature>();
		if (formFeature == null || formFeature.Form == null)
		{
			features.Set((IFormFeature)new FormFeature(request, options));
		}
		return request.ReadFormAsync(cancellationToken);
	}
}
