using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class RequestServicesContainerMiddleware
{
	private readonly RequestDelegate _next;

	private readonly IServiceScopeFactory _scopeFactory;

	public RequestServicesContainerMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
	{
		if (next == null)
		{
			throw new ArgumentNullException("next");
		}
		if (scopeFactory == null)
		{
			throw new ArgumentNullException("scopeFactory");
		}
		_next = next;
		_scopeFactory = scopeFactory;
	}

	public Task Invoke(HttpContext httpContext)
	{
		IFeatureCollection features = httpContext.Features;
		if (features.Get<IServiceProvidersFeature>()?.RequestServices != null)
		{
			return _next(httpContext);
		}
		features.Set((IServiceProvidersFeature)new RequestServicesFeature(httpContext, _scopeFactory));
		return _next(httpContext);
	}
}
