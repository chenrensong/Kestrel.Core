using System;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class AutoRequestServicesStartupFilter : IStartupFilter
{
	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
	{
		return delegate(IApplicationBuilder builder)
		{
			builder.UseMiddleware<RequestServicesContainerMiddleware>(Array.Empty<object>());
			next(builder);
		};
	}
}
