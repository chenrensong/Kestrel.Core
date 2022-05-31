using System;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

public static class UsePathBaseExtensions
{
	public static IApplicationBuilder UsePathBase(this IApplicationBuilder app, PathString pathBase)
	{
		if (app == null)
		{
			throw new ArgumentNullException("app");
		}
		pathBase = pathBase.Value?.TrimEnd(new char[1] { '/' });
		if (!pathBase.HasValue)
		{
			return app;
		}
		return app.UseMiddleware<UsePathBaseMiddleware>(new object[1] { pathBase });
	}
}
