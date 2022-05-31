using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder.Extensions;

public class MapWhenOptions
{
	private Func<HttpContext, bool> _predicate;

	public Func<HttpContext, bool> Predicate
	{
		get
		{
			return _predicate;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_predicate = value;
		}
	}

	public RequestDelegate Branch { get; set; }
}
