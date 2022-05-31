using System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Http;

public class HttpContextFactory : IHttpContextFactory
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly FormOptions _formOptions;

	public HttpContextFactory(IOptions<FormOptions> formOptions)
		: this(formOptions, null)
	{
	}

	public HttpContextFactory(IOptions<FormOptions> formOptions, IHttpContextAccessor httpContextAccessor)
	{
		if (formOptions == null)
		{
			throw new ArgumentNullException("formOptions");
		}
		_formOptions = formOptions.Value;
		_httpContextAccessor = httpContextAccessor;
	}

	public HttpContext Create(IFeatureCollection featureCollection)
	{
		if (featureCollection == null)
		{
			throw new ArgumentNullException("featureCollection");
		}
		DefaultHttpContext defaultHttpContext = new DefaultHttpContext(featureCollection);
		if (_httpContextAccessor != null)
		{
			_httpContextAccessor.HttpContext = defaultHttpContext;
		}
		FormFeature instance = new FormFeature(defaultHttpContext.Request, _formOptions);
		featureCollection.Set((IFormFeature)instance);
		return defaultHttpContext;
	}

	public void Dispose(HttpContext httpContext)
	{
		if (_httpContextAccessor != null)
		{
			_httpContextAccessor.HttpContext = null;
		}
	}
}
