using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http.Headers;

public class RequestHeaders
{
	public IHeaderDictionary Headers { get; }

	public IList<MediaTypeHeaderValue> Accept
	{
		get
		{
			return Headers.GetList<MediaTypeHeaderValue>("Accept");
		}
		set
		{
			Headers.SetList("Accept", value);
		}
	}

	public IList<StringWithQualityHeaderValue> AcceptCharset
	{
		get
		{
			return Headers.GetList<StringWithQualityHeaderValue>("Accept-Charset");
		}
		set
		{
			Headers.SetList("Accept-Charset", value);
		}
	}

	public IList<StringWithQualityHeaderValue> AcceptEncoding
	{
		get
		{
			return Headers.GetList<StringWithQualityHeaderValue>("Accept-Encoding");
		}
		set
		{
			Headers.SetList("Accept-Encoding", value);
		}
	}

	public IList<StringWithQualityHeaderValue> AcceptLanguage
	{
		get
		{
			return Headers.GetList<StringWithQualityHeaderValue>("Accept-Language");
		}
		set
		{
			Headers.SetList("Accept-Language", value);
		}
	}

	public CacheControlHeaderValue CacheControl
	{
		get
		{
			return Headers.Get<CacheControlHeaderValue>("Cache-Control");
		}
		set
		{
			Headers.Set("Cache-Control", value);
		}
	}

	public ContentDispositionHeaderValue ContentDisposition
	{
		get
		{
			return Headers.Get<ContentDispositionHeaderValue>("Content-Disposition");
		}
		set
		{
			Headers.Set("Content-Disposition", value);
		}
	}

	public long? ContentLength
	{
		get
		{
			return Headers.ContentLength;
		}
		set
		{
			Headers.ContentLength = value;
		}
	}

	public ContentRangeHeaderValue ContentRange
	{
		get
		{
			return Headers.Get<ContentRangeHeaderValue>("Content-Range");
		}
		set
		{
			Headers.Set("Content-Range", value);
		}
	}

	public MediaTypeHeaderValue ContentType
	{
		get
		{
			return Headers.Get<MediaTypeHeaderValue>("Content-Type");
		}
		set
		{
			Headers.Set("Content-Type", value);
		}
	}

	public IList<CookieHeaderValue> Cookie
	{
		get
		{
			return Headers.GetList<CookieHeaderValue>("Cookie");
		}
		set
		{
			Headers.SetList("Cookie", value);
		}
	}

	public DateTimeOffset? Date
	{
		get
		{
			return Headers.GetDate("Date");
		}
		set
		{
			Headers.SetDate("Date", value);
		}
	}

	public DateTimeOffset? Expires
	{
		get
		{
			return Headers.GetDate("Expires");
		}
		set
		{
			Headers.SetDate("Expires", value);
		}
	}

	public HostString Host
	{
		get
		{
			return HostString.FromUriComponent(Headers["Host"]);
		}
		set
		{
			Headers["Host"] = value.ToUriComponent();
		}
	}

	public IList<EntityTagHeaderValue> IfMatch
	{
		get
		{
			return Headers.GetList<EntityTagHeaderValue>("If-Match");
		}
		set
		{
			Headers.SetList("If-Match", value);
		}
	}

	public DateTimeOffset? IfModifiedSince
	{
		get
		{
			return Headers.GetDate("If-Modified-Since");
		}
		set
		{
			Headers.SetDate("If-Modified-Since", value);
		}
	}

	public IList<EntityTagHeaderValue> IfNoneMatch
	{
		get
		{
			return Headers.GetList<EntityTagHeaderValue>("If-None-Match");
		}
		set
		{
			Headers.SetList("If-None-Match", value);
		}
	}

	public RangeConditionHeaderValue IfRange
	{
		get
		{
			return Headers.Get<RangeConditionHeaderValue>("If-Range");
		}
		set
		{
			Headers.Set("If-Range", value);
		}
	}

	public DateTimeOffset? IfUnmodifiedSince
	{
		get
		{
			return Headers.GetDate("If-Unmodified-Since");
		}
		set
		{
			Headers.SetDate("If-Unmodified-Since", value);
		}
	}

	public DateTimeOffset? LastModified
	{
		get
		{
			return Headers.GetDate("Last-Modified");
		}
		set
		{
			Headers.SetDate("Last-Modified", value);
		}
	}

	public RangeHeaderValue Range
	{
		get
		{
			return Headers.Get<RangeHeaderValue>("Range");
		}
		set
		{
			Headers.Set("Range", value);
		}
	}

	public Uri Referer
	{
		get
		{
			if (Uri.TryCreate(Headers["Referer"], UriKind.RelativeOrAbsolute, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			Headers.Set("Referer", (value == null) ? null : UriHelper.Encode(value));
		}
	}

	public RequestHeaders(IHeaderDictionary headers)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		Headers = headers;
	}

	public T Get<T>(string name)
	{
		return Headers.Get<T>(name);
	}

	public IList<T> GetList<T>(string name)
	{
		return Headers.GetList<T>(name);
	}

	public void Set(string name, object value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Headers.Set(name, value);
	}

	public void SetList<T>(string name, IList<T> values)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Headers.SetList(name, values);
	}

	public void Append(string name, object value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Headers.Append(name, value.ToString());
	}

	public void AppendList<T>(string name, IList<T> values)
	{
		Headers.AppendList(name, values);
	}
}
