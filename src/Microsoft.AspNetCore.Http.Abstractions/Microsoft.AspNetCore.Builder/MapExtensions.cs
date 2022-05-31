using System;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

public static class MapExtensions
{
	public static IApplicationBuilder Map(this IApplicationBuilder app, PathString pathMatch,
		Action<IApplicationBuilder> configuration)
	{
		if (app == null)
		{
			throw new ArgumentNullException("app");
		}
		if (configuration == null)
		{
			throw new ArgumentNullException("configuration");
		}
		if (pathMatch.HasValue && pathMatch.Value.EndsWith("/", StringComparison.Ordinal))
		{
			throw new ArgumentException("The path must not end with a '/'", "pathMatch");
		}
		IApplicationBuilder applicationBuilder = app.New();
		configuration(applicationBuilder);
		RequestDelegate branch = applicationBuilder.Build();
		MapOptions options = new MapOptions
		{
			Branch = branch,
			PathMatch = pathMatch
		};
		return app.Use((RequestDelegate next) => new MapMiddleware(next, options).Invoke);
	}
}
