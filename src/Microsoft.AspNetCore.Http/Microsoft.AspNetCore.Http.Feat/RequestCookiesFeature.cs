using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http.Features;

public class RequestCookiesFeature : IRequestCookiesFeature
{
	private static readonly Func<IFeatureCollection, IHttpRequestFeature> _nullRequestFeature = (IFeatureCollection f) => null;

	private FeatureReferences<IHttpRequestFeature> _features;

	private StringValues _original;

	private IRequestCookieCollection _parsedValues;

	private IHttpRequestFeature HttpRequestFeature => _features.Fetch(ref _features.Cache, _nullRequestFeature);

	public IRequestCookieCollection Cookies
	{
		get
		{
			if (_features.Collection == null)
			{
				if (_parsedValues == null)
				{
					_parsedValues = RequestCookieCollection.Empty;
				}
				return _parsedValues;
			}
			if (!HttpRequestFeature.Headers.TryGetValue("Cookie", out var value))
			{
				value = string.Empty;
			}
			if (_parsedValues == null || _original != value)
			{
				_original = value;
				_parsedValues = RequestCookieCollection.Parse(value.ToArray());
			}
			return _parsedValues;
		}
		set
		{
			_parsedValues = value;
			_original = StringValues.Empty;
			if (_features.Collection == null)
			{
				return;
			}
			if (_parsedValues == null || _parsedValues.Count == 0)
			{
				HttpRequestFeature.Headers.Remove("Cookie");
				return;
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, string> parsedValue in _parsedValues)
			{
				list.Add(new CookieHeaderValue(parsedValue.Key, parsedValue.Value).ToString());
			}
			_original = list.ToArray();
			HttpRequestFeature.Headers["Cookie"] = _original;
		}
	}

	public RequestCookiesFeature(IRequestCookieCollection cookies)
	{
		if (cookies == null)
		{
			throw new ArgumentNullException("cookies");
		}
		_parsedValues = cookies;
	}

	public RequestCookiesFeature(IFeatureCollection features)
	{
		if (features == null)
		{
			throw new ArgumentNullException("features");
		}
		_features = new FeatureReferences<IHttpRequestFeature>(features);
	}
}
