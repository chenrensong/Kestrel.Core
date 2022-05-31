using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

public static class UseExtensions
{
	public static IApplicationBuilder Use(this IApplicationBuilder app, Func<HttpContext, Func<Task>, Task> middleware)
	{
		return app.Use((RequestDelegate next) => delegate(HttpContext context)
		{
			Func<Task> arg = () => next(context);
			return middleware(context, arg);
		});
	}
}
