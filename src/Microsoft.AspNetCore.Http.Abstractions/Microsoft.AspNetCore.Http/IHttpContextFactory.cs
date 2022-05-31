using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http;

public interface IHttpContextFactory
{
	HttpContext Create(IFeatureCollection featureCollection);

	void Dispose(HttpContext httpContext);
}
