using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpServiceCollectionExtensions
{
	public static IServiceCollection AddHttpContextAccessor(this IServiceCollection services)
	{
		if (services == null)
		{
			throw new ArgumentNullException("services");
		}
		services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		return services;
	}
}
