using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Internal;

public interface IStartupConfigureServicesFilter
{
	Action<IServiceCollection> ConfigureServices(Action<IServiceCollection> next);
}
