using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

public static class UseWhenExtensions
{
	public static IApplicationBuilder UseWhen(this IApplicationBuilder app, Func<HttpContext, bool> predicate, Action<IApplicationBuilder> configuration)
	{
		if (app == null)
		{
			throw new ArgumentNullException("app");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (configuration == null)
		{
			throw new ArgumentNullException("configuration");
		}
		IApplicationBuilder branchBuilder = app.New();
		configuration(branchBuilder);
		return app.Use(delegate(RequestDelegate main)
		{
			branchBuilder.Run(main);
			RequestDelegate branch = branchBuilder.Build();
			return (HttpContext context) => predicate(context) ? branch(context) : main(context);
		});
	}
}
