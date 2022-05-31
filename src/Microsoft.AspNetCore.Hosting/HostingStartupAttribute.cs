using System;
using System.Reflection;

namespace Microsoft.AspNetCore.Hosting;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public sealed class HostingStartupAttribute : Attribute
{
	public Type HostingStartupType { get; }

	public HostingStartupAttribute(Type hostingStartupType)
	{
		if (hostingStartupType == null)
		{
			throw new ArgumentNullException("hostingStartupType");
		}
		if (!typeof(IHostingStartup).GetTypeInfo().IsAssignableFrom(hostingStartupType.GetTypeInfo()))
		{
			throw new ArgumentException($"\"{hostingStartupType}\" does not implement {typeof(IHostingStartup)}.", "hostingStartupType");
		}
		HostingStartupType = hostingStartupType;
	}
}
