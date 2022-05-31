using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder.Extensions;

public class MapMiddleware
{
	private readonly RequestDelegate _next;

	private readonly MapOptions _options;

	public MapMiddleware(RequestDelegate next, MapOptions options)
	{
		if (next == null)
		{
			throw new ArgumentNullException("next");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		_next = next;
		_options = options;
	}

	public async Task Invoke(HttpContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (context.Request.Path.StartsWithSegments(_options.PathMatch, out var matched, out var remaining))
		{
			PathString path = context.Request.Path;
			PathString pathBase = context.Request.PathBase;
			context.Request.PathBase = pathBase.Add(matched);
			context.Request.Path = remaining;
			try
			{
				await _options.Branch(context);
			}
			finally
			{
				context.Request.PathBase = pathBase;
				context.Request.Path = path;
			}
		}
		else
		{
			await _next(context);
		}
	}
}
