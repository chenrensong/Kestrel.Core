using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting;

public class ConventionBasedStartup : IStartup
{
	private readonly StartupMethods _methods;

	public ConventionBasedStartup(StartupMethods methods)
	{
		_methods = methods;
	}

	public void Configure(IApplicationBuilder app)
	{
		try
		{
			_methods.ConfigureDelegate(app);
		}
		catch (Exception ex)
		{
			if (ex is TargetInvocationException)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
			throw;
		}
	}

	public IServiceProvider ConfigureServices(IServiceCollection services)
	{
		try
		{
			return _methods.ConfigureServicesDelegate(services);
		}
		catch (Exception ex)
		{
			if (ex is TargetInvocationException)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
			throw;
		}
	}
}
