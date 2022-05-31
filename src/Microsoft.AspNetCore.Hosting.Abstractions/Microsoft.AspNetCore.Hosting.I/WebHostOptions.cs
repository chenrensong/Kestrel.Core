using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class WebHostOptions
{
	public string ApplicationName { get; set; }

	public bool PreventHostingStartup { get; set; }

	public bool SuppressStatusMessages { get; set; }

	public IReadOnlyList<string> HostingStartupAssemblies { get; set; }

	public IReadOnlyList<string> HostingStartupExcludeAssemblies { get; set; }

	public bool DetailedErrors { get; set; }

	public bool CaptureStartupErrors { get; set; }

	public string Environment { get; set; }

	public string StartupAssembly { get; set; }

	public string WebRoot { get; set; }

	public string ContentRootPath { get; set; }

	public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5.0);


	public WebHostOptions()
	{
	}

	public WebHostOptions(IConfiguration configuration)
		: this(configuration, string.Empty)
	{
	}

	public WebHostOptions(IConfiguration configuration, string applicationNameFallback)
	{
		if (configuration == null)
		{
			throw new ArgumentNullException("configuration");
		}
		ApplicationName = configuration[WebHostDefaults.ApplicationKey] ?? applicationNameFallback;
		StartupAssembly = configuration[WebHostDefaults.StartupAssemblyKey];
		DetailedErrors = WebHostUtilities.ParseBool(configuration, WebHostDefaults.DetailedErrorsKey);
		CaptureStartupErrors = WebHostUtilities.ParseBool(configuration, WebHostDefaults.CaptureStartupErrorsKey);
		Environment = configuration[WebHostDefaults.EnvironmentKey];
		WebRoot = configuration[WebHostDefaults.WebRootKey];
		ContentRootPath = configuration[WebHostDefaults.ContentRootKey];
		PreventHostingStartup = WebHostUtilities.ParseBool(configuration, WebHostDefaults.PreventHostingStartupKey);
		SuppressStatusMessages = WebHostUtilities.ParseBool(configuration, WebHostDefaults.SuppressStatusMessagesKey);
		HostingStartupAssemblies = Split(ApplicationName + ";" + configuration[WebHostDefaults.HostingStartupAssembliesKey]);
		HostingStartupExcludeAssemblies = Split(configuration[WebHostDefaults.HostingStartupExcludeAssembliesKey]);
		string text = configuration[WebHostDefaults.ShutdownTimeoutKey];
		if (!string.IsNullOrEmpty(text) && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var result))
		{
			ShutdownTimeout = TimeSpan.FromSeconds(result);
		}
	}

	public IEnumerable<string> GetFinalHostingStartupAssemblies()
	{
		return HostingStartupAssemblies.Except(HostingStartupExcludeAssemblies, StringComparer.OrdinalIgnoreCase);
	}

	private IReadOnlyList<string> Split(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return Array.Empty<string>();
		}
		List<string> list = new List<string>();
		string[] array = value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(text);
			}
		}
		return list;
	}
}
