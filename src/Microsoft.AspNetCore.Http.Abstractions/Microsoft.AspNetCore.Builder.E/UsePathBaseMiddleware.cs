using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder.Extensions;

public class UsePathBaseMiddleware
{
	private readonly RequestDelegate _next;

	private readonly PathString _pathBase;

	public UsePathBaseMiddleware(RequestDelegate next, PathString pathBase)
	{
		if (next == null)
		{
			throw new ArgumentNullException("next");
		}
		if (!pathBase.HasValue)
		{
			throw new ArgumentException("pathBase cannot be null or empty.");
		}
		_next = next;
		_pathBase = pathBase;
	}

	public async Task Invoke(HttpContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (context.Request.Path.StartsWithSegments(_pathBase, out var matched, out var remaining))
		{
			PathString originalPath = context.Request.Path;
			PathString originalPathBase = context.Request.PathBase;
			context.Request.Path = remaining;
			context.Request.PathBase = originalPathBase.Add(matched);
			try
			{
				await _next(context);
			}
			finally
			{
				context.Request.Path = originalPath;
				context.Request.PathBase = originalPathBase;
			}
		}
		else
		{
			await _next(context);
		}
	}
}
