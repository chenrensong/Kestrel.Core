using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Internal;

internal static class ServiceCollectionExtensions
{
	public static IServiceCollection Clone(this IServiceCollection serviceCollection)
	{
		IServiceCollection serviceCollection2 = new ServiceCollection();
		foreach (ServiceDescriptor item in serviceCollection)
		{
			serviceCollection2.Add(item);
		}
		return serviceCollection2;
	}
}
