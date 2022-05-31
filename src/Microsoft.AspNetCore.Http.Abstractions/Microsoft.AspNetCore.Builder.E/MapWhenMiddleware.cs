using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder.Extensions;

public class MapWhenMiddleware
{
	private readonly RequestDelegate _next;

	private readonly MapWhenOptions _options;

	public MapWhenMiddleware(RequestDelegate next, MapWhenOptions options)
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
		if (_options.Predicate(context))
		{
			await _options.Branch(context);
		}
		else
		{
			await _next(context);
		}
	}
}
