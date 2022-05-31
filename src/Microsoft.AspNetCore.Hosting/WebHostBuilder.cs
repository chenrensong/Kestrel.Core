using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.Hosting;

public class WebHostBuilder : IWebHostBuilder
{
	private readonly HostingEnvironment _hostingEnvironment;

	private readonly List<Action<WebHostBuilderContext, IServiceCollection>> _configureServicesDelegates;

	private IConfiguration _config;

	private WebHostOptions _options;

	private WebHostBuilderContext _context;

	private bool _webHostBuilt;

	private List<Action<WebHostBuilderContext, IConfigurationBuilder>> _configureAppConfigurationBuilderDelegates;

	public WebHostBuilder()
	{
		_hostingEnvironment = new HostingEnvironment();
		_configureServicesDelegates = new List<Action<WebHostBuilderContext, IServiceCollection>>();
		_configureAppConfigurationBuilderDelegates = new List<Action<WebHostBuilderContext, IConfigurationBuilder>>();
		_config = new ConfigurationBuilder().AddEnvironmentVariables("ASPNETCORE_").Build();
		if (string.IsNullOrEmpty(GetSetting(WebHostDefaults.EnvironmentKey)))
		{
			UseSetting(WebHostDefaults.EnvironmentKey, Environment.GetEnvironmentVariable("Hosting:Environment") ?? Environment.GetEnvironmentVariable("ASPNET_ENV"));
		}
		if (string.IsNullOrEmpty(GetSetting(WebHostDefaults.ServerUrlsKey)))
		{
			UseSetting(WebHostDefaults.ServerUrlsKey, Environment.GetEnvironmentVariable("ASPNETCORE_SERVER.URLS"));
		}
		_context = new WebHostBuilderContext
		{
			Configuration = _config
		};
	}

	public string GetSetting(string key)
	{
		return _config[key];
	}

	public IWebHostBuilder UseSetting(string key, string value)
	{
		_config[key] = value;
		return this;
	}

	public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
	{
		if (configureServices == null)
		{
			throw new ArgumentNullException("configureServices");
		}
		return ConfigureServices(delegate(WebHostBuilderContext _, IServiceCollection services)
		{
			configureServices(services);
		});
	}

	public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
	{
		if (configureServices == null)
		{
			throw new ArgumentNullException("configureServices");
		}
		_configureServicesDelegates.Add(configureServices);
		return this;
	}

	public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
	{
		if (configureDelegate == null)
		{
			throw new ArgumentNullException("configureDelegate");
		}
		_configureAppConfigurationBuilderDelegates.Add(configureDelegate);
		return this;
	}

	public IWebHost Build()
	{
		if (_webHostBuilt)
		{
			throw new InvalidOperationException(Resources.WebHostBuilder_SingleInstance);
		}
		_webHostBuilt = true;
		AggregateException hostingStartupErrors;
		IServiceCollection serviceCollection = BuildCommonServices(out hostingStartupErrors);
		IServiceCollection serviceCollection2 = serviceCollection.Clone();
		IServiceProvider hostingServiceProvider = GetProviderFromFactory(serviceCollection);
		if (!_options.SuppressStatusMessages)
		{
			if (Environment.GetEnvironmentVariable("Hosting:Environment") != null)
			{
				Console.WriteLine("The environment variable 'Hosting:Environment' is obsolete and has been replaced with 'ASPNETCORE_ENVIRONMENT'");
			}
			if (Environment.GetEnvironmentVariable("ASPNET_ENV") != null)
			{
				Console.WriteLine("The environment variable 'ASPNET_ENV' is obsolete and has been replaced with 'ASPNETCORE_ENVIRONMENT'");
			}
			if (Environment.GetEnvironmentVariable("ASPNETCORE_SERVER.URLS") != null)
			{
				Console.WriteLine("The environment variable 'ASPNETCORE_SERVER.URLS' is obsolete and has been replaced with 'ASPNETCORE_URLS'");
			}
		}
		AddApplicationServices(serviceCollection2, hostingServiceProvider);
		WebHost webHost = new WebHost(serviceCollection2, hostingServiceProvider, _options, _config, hostingStartupErrors);
		try
		{
			webHost.Initialize();
			ILogger<WebHost> requiredService = ServiceProviderServiceExtensions.GetRequiredService<ILogger<WebHost>>(webHost.Services);
			foreach (IGrouping<string, string> item in from g in _options.GetFinalHostingStartupAssemblies().GroupBy((string a) => a, StringComparer.OrdinalIgnoreCase)
				where g.Count() > 1
				select g)
			{
				requiredService.LogWarning($"The assembly {item} was specified multiple times. Hosting startup assemblies should only be specified once.");
			}
			return webHost;
		}
		catch
		{
			webHost.Dispose();
			throw;
		}
		static IServiceProvider GetProviderFromFactory(IServiceCollection collection)
		{
			ServiceProvider serviceProvider = collection.BuildServiceProvider();
			IServiceProviderFactory<IServiceCollection> service = ServiceProviderServiceExtensions.GetService<IServiceProviderFactory<IServiceCollection>>(serviceProvider);
			if (service != null && !(service is DefaultServiceProviderFactory))
			{
				using (serviceProvider)
				{
					return service.CreateServiceProvider(service.CreateBuilder(collection));
				}
			}
			return serviceProvider;
		}
	}

	private IServiceCollection BuildCommonServices(out AggregateException hostingStartupErrors)
	{
		hostingStartupErrors = null;
		_options = new WebHostOptions(_config, Assembly.GetEntryAssembly()?.GetName().Name);
		if (!_options.PreventHostingStartup)
		{
			List<Exception> list = new List<Exception>();
			foreach (string item in _options.GetFinalHostingStartupAssemblies().Distinct(StringComparer.OrdinalIgnoreCase))
			{
				try
				{
					foreach (HostingStartupAttribute customAttribute in Assembly.Load(new AssemblyName(item)).GetCustomAttributes<HostingStartupAttribute>())
					{
						((IHostingStartup)Activator.CreateInstance(customAttribute.HostingStartupType)).Configure(this);
					}
				}
				catch (Exception innerException)
				{
					list.Add(new InvalidOperationException("Startup assembly " + item + " failed to execute. See the inner exception for more details.", innerException));
				}
			}
			if (list.Count > 0)
			{
				hostingStartupErrors = new AggregateException(list);
			}
		}
		string contentRootPath = ResolveContentRootPath(_options.ContentRootPath, AppContext.BaseDirectory);
		_hostingEnvironment.Initialize(contentRootPath, _options);
		_context.HostingEnvironment = _hostingEnvironment;
		ServiceCollection serviceCollection = new ServiceCollection();
		ServiceCollectionServiceExtensions.AddSingleton(serviceCollection, _options);
		ServiceCollectionServiceExtensions.AddSingleton((IServiceCollection)serviceCollection, (IHostEnvironment)_hostingEnvironment);
		ServiceCollectionServiceExtensions.AddSingleton((IServiceCollection)serviceCollection, (Microsoft.Extensions.Hosting.IHostEnvironment)_hostingEnvironment);
		ServiceCollectionServiceExtensions.AddSingleton(serviceCollection, _context);
		IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(_hostingEnvironment.ContentRootPath).AddConfiguration(_config);
		foreach (Action<WebHostBuilderContext, IConfigurationBuilder> configureAppConfigurationBuilderDelegate in _configureAppConfigurationBuilderDelegates)
		{
			configureAppConfigurationBuilderDelegate(_context, configurationBuilder);
		}
		IConfigurationRoot configurationRoot = configurationBuilder.Build();
		ServiceCollectionServiceExtensions.AddSingleton((IServiceCollection)serviceCollection, (IConfiguration)configurationRoot);
		_context.Configuration = configurationRoot;
		DiagnosticListener implementationInstance = new DiagnosticListener("Microsoft.AspNetCore");
		ServiceCollectionServiceExtensions.AddSingleton(serviceCollection, implementationInstance);
		ServiceCollectionServiceExtensions.AddSingleton((IServiceCollection)serviceCollection, (DiagnosticSource)implementationInstance);
		ServiceCollectionServiceExtensions.AddTransient<IApplicationBuilderFactory, ApplicationBuilderFactory>(serviceCollection);
		ServiceCollectionServiceExtensions.AddTransient<IHttpContextFactory, HttpContextFactory>(serviceCollection);
		ServiceCollectionServiceExtensions.AddScoped<IMiddlewareFactory, MiddlewareFactory>(serviceCollection);
		serviceCollection.AddOptions();
		serviceCollection.AddLogging();
		ServiceCollectionServiceExtensions.AddTransient<IStartupFilter, AutoRequestServicesStartupFilter>(serviceCollection);
		ServiceCollectionServiceExtensions.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>(serviceCollection);
		ServiceCollectionServiceExtensions.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>(serviceCollection);
		if (!string.IsNullOrEmpty(_options.StartupAssembly))
		{
			try
			{
				Type startupType = StartupLoader.FindStartupType(_options.StartupAssembly, _hostingEnvironment.EnvironmentName);
				if (typeof(IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
				{
					ServiceCollectionServiceExtensions.AddSingleton(serviceCollection, typeof(IStartup), startupType);
				}
				else
				{
					ServiceCollectionServiceExtensions.AddSingleton(serviceCollection, typeof(IStartup), delegate(IServiceProvider sp)
					{
						IHostEnvironment requiredService = ServiceProviderServiceExtensions.GetRequiredService<IHostEnvironment>(sp);
						return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType, requiredService.EnvironmentName));
					});
				}
			}
			catch (Exception source)
			{
				ExceptionDispatchInfo capture = ExceptionDispatchInfo.Capture(source);
				ServiceCollectionServiceExtensions.AddSingleton((IServiceCollection)serviceCollection, (Func<IServiceProvider, IStartup>)delegate
				{
					capture.Throw();
					return null;
				});
			}
		}
		foreach (Action<WebHostBuilderContext, IServiceCollection> configureServicesDelegate in _configureServicesDelegates)
		{
			configureServicesDelegate(_context, serviceCollection);
		}
		return serviceCollection;
	}

	private void AddApplicationServices(IServiceCollection services, IServiceProvider hostingServiceProvider)
	{
		DiagnosticListener service = ServiceProviderServiceExtensions.GetService<DiagnosticListener>(hostingServiceProvider);
		ServiceCollectionDescriptorExtensions.Replace(services, ServiceDescriptor.Singleton(typeof(DiagnosticListener), service));
		ServiceCollectionDescriptorExtensions.Replace(services, ServiceDescriptor.Singleton(typeof(DiagnosticSource), service));
	}

	private string ResolveContentRootPath(string contentRootPath, string basePath)
	{
		if (string.IsNullOrEmpty(contentRootPath))
		{
			return basePath;
		}
		if (Path.IsPathRooted(contentRootPath))
		{
			return contentRootPath;
		}
		return Path.Combine(Path.GetFullPath(basePath), contentRootPath);
	}
}
