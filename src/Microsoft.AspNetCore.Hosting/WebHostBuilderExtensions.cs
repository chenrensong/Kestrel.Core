using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Hosting;

public static class WebHostBuilderExtensions
{
	public static IWebHostBuilder Configure(this IWebHostBuilder hostBuilder, Action<IApplicationBuilder> configureApp)
	{
		if (configureApp == null)
		{
			throw new ArgumentNullException("configureApp");
		}
		string name = RuntimeReflectionExtensions.GetMethodInfo(configureApp).DeclaringType.GetTypeInfo().Assembly.GetName().Name;
		return hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, name).ConfigureServices(delegate(IServiceCollection services)
		{
			ServiceCollectionServiceExtensions.AddSingleton(services, (Func<IServiceProvider, IStartup>)((IServiceProvider sp) => new DelegateStartup(ServiceProviderServiceExtensions.GetRequiredService<IServiceProviderFactory<IServiceCollection>>(sp), configureApp)));
		});
	}

	public static IWebHostBuilder UseStartup(this IWebHostBuilder hostBuilder, Type startupType)
	{
		string name = startupType.GetTypeInfo().Assembly.GetName().Name;
		return hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, name).ConfigureServices(delegate(IServiceCollection services)
		{
			if (typeof(IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
			{
				ServiceCollectionServiceExtensions.AddSingleton(services, typeof(IStartup), startupType);
			}
			else
			{
				ServiceCollectionServiceExtensions.AddSingleton(services, typeof(IStartup), delegate(IServiceProvider sp)
				{
					IHostEnvironment requiredService = ServiceProviderServiceExtensions.GetRequiredService<IHostEnvironment>(sp);
					return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType, requiredService.EnvironmentName));
				});
			}
		});
	}

	public static IWebHostBuilder UseStartup<TStartup>(this IWebHostBuilder hostBuilder) where TStartup : class
	{
		return hostBuilder.UseStartup(typeof(TStartup));
	}

	public static IWebHostBuilder UseDefaultServiceProvider(this IWebHostBuilder hostBuilder, Action<ServiceProviderOptions> configure)
	{
		return hostBuilder.UseDefaultServiceProvider(delegate(WebHostBuilderContext context, ServiceProviderOptions options)
		{
			configure(options);
		});
	}

	public static IWebHostBuilder UseDefaultServiceProvider(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, ServiceProviderOptions> configure)
	{
		return hostBuilder.ConfigureServices(delegate(WebHostBuilderContext context, IServiceCollection services)
		{
			ServiceProviderOptions serviceProviderOptions = new ServiceProviderOptions();
			configure(context, serviceProviderOptions);
			ServiceCollectionDescriptorExtensions.Replace(services, ServiceDescriptor.Singleton((IServiceProviderFactory<IServiceCollection>)new DefaultServiceProviderFactory(serviceProviderOptions)));
		});
	}

	public static IWebHostBuilder ConfigureAppConfiguration(this IWebHostBuilder hostBuilder, Action<IConfigurationBuilder> configureDelegate)
	{
		return hostBuilder.ConfigureAppConfiguration(delegate(WebHostBuilderContext context, IConfigurationBuilder builder)
		{
			configureDelegate(builder);
		});
	}

	public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder hostBuilder, Action<ILoggingBuilder> configureLogging)
	{
		return hostBuilder.ConfigureServices(delegate(IServiceCollection collection)
		{
			collection.AddLogging(configureLogging);
		});
	}

	public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, ILoggingBuilder> configureLogging)
	{
		return hostBuilder.ConfigureServices(delegate(WebHostBuilderContext context, IServiceCollection collection)
		{
			collection.AddLogging(delegate(ILoggingBuilder builder)
			{
				configureLogging(context, builder);
			});
		});
	}
}
