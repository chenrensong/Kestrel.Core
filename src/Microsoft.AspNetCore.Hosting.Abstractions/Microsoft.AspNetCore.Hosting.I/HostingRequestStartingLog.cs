using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Hosting.Internal;

internal class HostingRequestStartingLog : IReadOnlyList<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IReadOnlyCollection<KeyValuePair<string, object>>
{
	internal static readonly Func<object, Exception, string> Callback = (object state, Exception exception) => ((HostingRequestStartingLog)state).ToString();

	private readonly HttpRequest _request;

	private string _cachedToString;

	public int Count => 9;

	public KeyValuePair<string, object> this[int index] => index switch
	{
		0 => new KeyValuePair<string, object>("Protocol", _request.Protocol), 
		1 => new KeyValuePair<string, object>("Method", _request.Method), 
		2 => new KeyValuePair<string, object>("ContentType", _request.ContentType), 
		3 => new KeyValuePair<string, object>("ContentLength", _request.ContentLength), 
		4 => new KeyValuePair<string, object>("Scheme", _request.Scheme.ToString()), 
		5 => new KeyValuePair<string, object>("Host", _request.Host.ToString()), 
		6 => new KeyValuePair<string, object>("PathBase", _request.PathBase.ToString()), 
		7 => new KeyValuePair<string, object>("Path", _request.Path.ToString()), 
		8 => new KeyValuePair<string, object>("QueryString", _request.QueryString.ToString()), 
		_ => throw new IndexOutOfRangeException("index"), 
	};

	public HostingRequestStartingLog(HttpContext httpContext)
	{
		_request = httpContext.Request;
	}

	public override string ToString()
	{
		if (_cachedToString == null)
		{
			_cachedToString = string.Format(CultureInfo.InvariantCulture, "Request starting {0} {1} {2}://{3}{4}{5}{6} {7} {8}", _request.Protocol, _request.Method, _request.Scheme, _request.Host.Value, _request.PathBase.Value, _request.Path.Value, _request.QueryString.Value, _request.ContentType, _request.ContentLength);
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
