using System;

namespace Microsoft.AspNetCore.Http.Features;

public interface IServiceProvidersFeature
{
	IServiceProvider RequestServices { get; set; }
}
