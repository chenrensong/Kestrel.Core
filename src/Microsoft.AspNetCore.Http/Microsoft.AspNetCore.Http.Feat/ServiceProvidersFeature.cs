using System;

namespace Microsoft.AspNetCore.Http.Features;

public class ServiceProvidersFeature : IServiceProvidersFeature
{
	public IServiceProvider RequestServices { get; set; }
}
