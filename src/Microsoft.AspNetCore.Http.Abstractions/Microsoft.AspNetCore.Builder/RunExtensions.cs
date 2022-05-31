using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

public static class RunExtensions
{
	public static void Run(this IApplicationBuilder app, RequestDelegate handler)
	{
		if (app == null)
		{
			throw new ArgumentNullException("app");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		app.Use((RequestDelegate _) => handler);
	}
}
