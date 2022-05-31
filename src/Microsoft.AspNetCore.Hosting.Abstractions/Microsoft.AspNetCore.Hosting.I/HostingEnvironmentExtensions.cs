using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.Hosting.Internal;

public static class HostingEnvironmentExtensions
{
	public static void Initialize(this IHostEnvironment hostingEnvironment, string contentRootPath, WebHostOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (string.IsNullOrEmpty(contentRootPath))
		{
			throw new ArgumentException("A valid non-empty content root must be provided.", "contentRootPath");
		}
		if (!Directory.Exists(contentRootPath))
		{
			throw new ArgumentException("The content root '" + contentRootPath + "' does not exist.", "contentRootPath");
		}
		hostingEnvironment.ApplicationName = options.ApplicationName;
		hostingEnvironment.ContentRootPath = contentRootPath;
		hostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(hostingEnvironment.ContentRootPath);
		string webRoot = options.WebRoot;
		if (webRoot == null)
		{
			string text = Path.Combine(hostingEnvironment.ContentRootPath, "wwwroot");
			if (Directory.Exists(text))
			{
                hostingEnvironment.ContentRootPath = text;
			}
		}
		else
		{
			hostingEnvironment.ContentRootPath = Path.Combine(hostingEnvironment.ContentRootPath, webRoot);
		}
		if (!string.IsNullOrEmpty(hostingEnvironment.ContentRootPath))
		{
			hostingEnvironment.ContentRootPath = Path.GetFullPath(hostingEnvironment.ContentRootPath);
			if (!Directory.Exists(hostingEnvironment.ContentRootPath))
			{
				Directory.CreateDirectory(hostingEnvironment.ContentRootPath);
			}

            hostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(hostingEnvironment.ContentRootPath);
		}
		else
		{
			hostingEnvironment.ContentRootFileProvider = new NullFileProvider();
		}
		hostingEnvironment.EnvironmentName = options.Environment ?? hostingEnvironment.EnvironmentName;
	}
}
