using Microsoft.Extensions.Hosting;
using System;

namespace Microsoft.AspNetCore.Hosting;

public static class HostingEnvironmentExtensions
{
	public static bool IsDevelopment(this IHostEnvironment hostingEnvironment)
	{
		if (hostingEnvironment == null)
		{
			throw new ArgumentNullException("hostingEnvironment");
		}
		return hostingEnvironment.IsEnvironment(EnvironmentName.Development);
	}

	public static bool IsStaging(this IHostEnvironment hostingEnvironment)
	{
		if (hostingEnvironment == null)
		{
			throw new ArgumentNullException("hostingEnvironment");
		}
		return hostingEnvironment.IsEnvironment(EnvironmentName.Staging);
	}

	public static bool IsProduction(this IHostEnvironment hostingEnvironment)
	{
		if (hostingEnvironment == null)
		{
			throw new ArgumentNullException("hostingEnvironment");
		}
		return hostingEnvironment.IsEnvironment(EnvironmentName.Production);
	}

	public static bool IsEnvironment(this IHostEnvironment hostingEnvironment, string environmentName)
	{
		if (hostingEnvironment == null)
		{
			throw new ArgumentNullException("hostingEnvironment");
		}
		return string.Equals(hostingEnvironment.EnvironmentName, environmentName, StringComparison.OrdinalIgnoreCase);
	}
}
