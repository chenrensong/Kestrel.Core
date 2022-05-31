using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Builder.Internal;

public class ApplicationBuilder : IApplicationBuilder
{
	private readonly IList<Func<RequestDelegate, RequestDelegate>> _components = new List<Func<RequestDelegate, RequestDelegate>>();

	public IServiceProvider ApplicationServices
	{
		get
		{
			return GetProperty<IServiceProvider>(Constants.BuilderProperties.ApplicationServices);
		}
		set
		{
			SetProperty(Constants.BuilderProperties.ApplicationServices, value);
		}
	}

	public IFeatureCollection ServerFeatures => GetProperty<IFeatureCollection>(Constants.BuilderProperties.ServerFeatures);

	public IDictionary<string, object> Properties { get; }

	public ApplicationBuilder(IServiceProvider serviceProvider)
	{
		Properties = new Dictionary<string, object>(StringComparer.Ordinal);
		ApplicationServices = serviceProvider;
	}

	public ApplicationBuilder(IServiceProvider serviceProvider, object server)
		: this(serviceProvider)
	{
		SetProperty(Constants.BuilderProperties.ServerFeatures, server);
	}

	private ApplicationBuilder(ApplicationBuilder builder)
	{
		Properties = new CopyOnWriteDictionary<string, object>(builder.Properties, StringComparer.Ordinal);
	}

	private T GetProperty<T>(string key)
	{
		if (!Properties.TryGetValue(key, out var value))
		{
			return default(T);
		}
		return (T)value;
	}

	private void SetProperty<T>(string key, T value)
	{
		Properties[key] = value;
	}

	public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
	{
		_components.Add(middleware);
		return this;
	}

	public IApplicationBuilder New()
	{
		return new ApplicationBuilder(this);
	}

	public RequestDelegate Build()
	{
		RequestDelegate requestDelegate = delegate(HttpContext context)
		{
			context.Response.StatusCode = 404;
			return Task.CompletedTask;
		};
		foreach (Func<RequestDelegate, RequestDelegate> item in _components.Reverse())
		{
			requestDelegate = item(requestDelegate);
		}
		return requestDelegate;
	}
}
