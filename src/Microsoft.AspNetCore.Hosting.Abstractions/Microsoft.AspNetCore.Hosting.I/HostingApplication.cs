using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class HostingApplication : IHttpApplication<HostingApplication.Context>
{
	public struct Context
	{
		public HttpContext HttpContext { get; set; }

		public IDisposable Scope { get; set; }

		public long StartTimestamp { get; set; }

		public bool EventLogEnabled { get; set; }

		public Activity Activity { get; set; }
	}

	private readonly RequestDelegate _application;

	private readonly IHttpContextFactory _httpContextFactory;

	private HostingApplicationDiagnostics _diagnostics;

	public HostingApplication(RequestDelegate application, ILogger logger, DiagnosticListener diagnosticSource, IHttpContextFactory httpContextFactory)
	{
		_application = application;
		_diagnostics = new HostingApplicationDiagnostics(logger, diagnosticSource);
		_httpContextFactory = httpContextFactory;
	}

	public Context CreateContext(IFeatureCollection contextFeatures)
	{
		Context context = default(Context);
		HttpContext httpContext = _httpContextFactory.Create(contextFeatures);
		_diagnostics.BeginRequest(httpContext, ref context);
		context.HttpContext = httpContext;
		return context;
	}

	public Task ProcessRequestAsync(Context context)
	{
		return _application(context.HttpContext);
	}

	public void DisposeContext(Context context, Exception exception)
	{
		HttpContext httpContext = context.HttpContext;
		_diagnostics.RequestEnd(httpContext, exception, context);
		_httpContextFactory.Dispose(httpContext);
		_diagnostics.ContextDisposed(context);
	}
}
