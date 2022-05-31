using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class RequestServicesFeature : IServiceProvidersFeature, IDisposable
{
	private readonly IServiceScopeFactory _scopeFactory;

	private IServiceProvider _requestServices;

	private IServiceScope _scope;

	private bool _requestServicesSet;

	private HttpContext _context;

	public IServiceProvider RequestServices
	{
		get
		{
			if (!_requestServicesSet)
			{
				_context.Response.RegisterForDispose(this);
				_scope = _scopeFactory.CreateScope();
				_requestServices = _scope.ServiceProvider;
				_requestServicesSet = true;
			}
			return _requestServices;
		}
		set
		{
			_requestServices = value;
			_requestServicesSet = true;
		}
	}

	public RequestServicesFeature(HttpContext context, IServiceScopeFactory scopeFactory)
	{
		_context = context;
		_scopeFactory = scopeFactory;
	}

	public void Dispose()
	{
		_scope?.Dispose();
		_scope = null;
		_requestServices = null;
	}
}
