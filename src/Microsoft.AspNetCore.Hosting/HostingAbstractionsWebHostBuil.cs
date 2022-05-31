using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting;

public static class HostingAbstractionsWebHostBuilderExtensions
{
	private static readonly string ServerUrlsSeparator = ";";

	public static IWebHostBuilder UseConfiguration(this IWebHostBuilder hostBuilder, IConfiguration configuration)
	{
		foreach (KeyValuePair<string, string> item in configuration.AsEnumerable())
		{
			hostBuilder.UseSetting(item.Key, item.Value);
		}
		return hostBuilder;
	}

	public static IWebHostBuilder CaptureStartupErrors(this IWebHostBuilder hostBuilder, bool captureStartupErrors)
	{
		return hostBuilder.UseSetting(WebHostDefaults.CaptureStartupErrorsKey, captureStartupErrors ? "true" : "false");
	}

	public static IWebHostBuilder UseStartup(this IWebHostBuilder hostBuilder, string startupAssemblyName)
	{
		if (startupAssemblyName == null)
		{
			throw new ArgumentNullException("startupAssemblyName");
		}
		return hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName).UseSetting(WebHostDefaults.StartupAssemblyKey, startupAssemblyName);
	}

	public static IWebHostBuilder UseServer(this IWebHostBuilder hostBuilder, IServer server)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		return hostBuilder.ConfigureServices(delegate(IServiceCollection services)
		{
			services.AddSingleton(server);
		});
	}

	public static IWebHostBuilder UseEnvironment(this IWebHostBuilder hostBuilder, string environment)
	{
		if (environment == null)
		{
			throw new ArgumentNullException("environment");
		}
		return hostBuilder.UseSetting(WebHostDefaults.EnvironmentKey, environment);
	}

	public static IWebHostBuilder UseContentRoot(this IWebHostBuilder hostBuilder, string contentRoot)
	{
		if (contentRoot == null)
		{
			throw new ArgumentNullException("contentRoot");
		}
		return hostBuilder.UseSetting(WebHostDefaults.ContentRootKey, contentRoot);
	}

	public static IWebHostBuilder UseWebRoot(this IWebHostBuilder hostBuilder, string webRoot)
	{
		if (webRoot == null)
		{
			throw new ArgumentNullException("webRoot");
		}
		return hostBuilder.UseSetting(WebHostDefaults.WebRootKey, webRoot);
	}

	public static IWebHostBuilder UseUrls(this IWebHostBuilder hostBuilder, params string[] urls)
	{
		if (urls == null)
		{
			throw new ArgumentNullException("urls");
		}
		return hostBuilder.UseSetting(WebHostDefaults.ServerUrlsKey, string.Join(ServerUrlsSeparator, urls));
	}

	public static IWebHostBuilder PreferHostingUrls(this IWebHostBuilder hostBuilder, bool preferHostingUrls)
	{
		return hostBuilder.UseSetting(WebHostDefaults.PreferHostingUrlsKey, preferHostingUrls ? "true" : "false");
	}

	public static IWebHostBuilder SuppressStatusMessages(this IWebHostBuilder hostBuilder, bool suppressStatusMessages)
	{
		return hostBuilder.UseSetting(WebHostDefaults.SuppressStatusMessagesKey, suppressStatusMessages ? "true" : "false");
	}

	public static IWebHostBuilder UseShutdownTimeout(this IWebHostBuilder hostBuilder, TimeSpan timeout)
	{
		return hostBuilder.UseSetting(WebHostDefaults.ShutdownTimeoutKey, ((int)timeout.TotalSeconds).ToString(CultureInfo.InvariantCulture));
	}

	public static IWebHost Start(this IWebHostBuilder hostBuilder, params string[] urls)
	{
        IWebHost webHost = hostBuilder.UseUrls(urls).Build();
		webHost.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
		return webHost;
	}
}
