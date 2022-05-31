using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class StartupMethods
{
	public object StartupInstance { get; }

	public Func<IServiceCollection, IServiceProvider> ConfigureServicesDelegate { get; }

	public Action<IApplicationBuilder> ConfigureDelegate { get; }

	public StartupMethods(object instance, Action<IApplicationBuilder> configure, Func<IServiceCollection, IServiceProvider> configureServices)
	{
		StartupInstance = instance;
		ConfigureDelegate = configure;
		ConfigureServicesDelegate = configureServices;
	}
}
