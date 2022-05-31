using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Hosting.Internal;

internal class HostingRequestFinishedLog : IReadOnlyList<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IReadOnlyCollection<KeyValuePair<string, object>>
{
	internal static readonly Func<object, Exception, string> Callback = (object state, Exception exception) => ((HostingRequestFinishedLog)state).ToString();

	private readonly HttpContext _httpContext;

	private readonly TimeSpan _elapsed;

	private string _cachedToString;

	public int Count => 3;

	public KeyValuePair<string, object> this[int index] => index switch
	{
		0 => new KeyValuePair<string, object>("ElapsedMilliseconds", _elapsed.TotalMilliseconds), 
		1 => new KeyValuePair<string, object>("StatusCode", _httpContext.Response.StatusCode), 
		2 => new KeyValuePair<string, object>("ContentType", _httpContext.Response.ContentType), 
		_ => throw new IndexOutOfRangeException("index"), 
	};

	public HostingRequestFinishedLog(HttpContext httpContext, TimeSpan elapsed)
	{
		_httpContext = httpContext;
		_elapsed = elapsed;
	}

	public override string ToString()
	{
		if (_cachedToString == null)
		{
			_cachedToString = string.Format(CultureInfo.InvariantCulture, "Request finished in {0}ms {1} {2}", _elapsed.TotalMilliseconds, _httpContext.Response.StatusCode, _httpContext.Response.ContentType);
		}
		return _cachedToString;
	}

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
