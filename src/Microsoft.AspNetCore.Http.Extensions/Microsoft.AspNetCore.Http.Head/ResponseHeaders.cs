using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http.Headers;

public class ResponseHeaders
{
	public IHeaderDictionary Headers { get; }

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

	public EntityTagHeaderValue ETag
	{
		get
		{
			return Headers.Get<EntityTagHeaderValue>("ETag");
		}
		set
		{
			Headers.Set("ETag", value);
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

	public Uri Location
	{
		get
		{
			if (Uri.TryCreate(Headers["Location"], UriKind.RelativeOrAbsolute, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			Headers.Set("Location", (value == null) ? null : UriHelper.Encode(value));
		}
	}

	public IList<SetCookieHeaderValue> SetCookie
	{
		get
		{
			return Headers.GetList<SetCookieHeaderValue>("Set-Cookie");
		}
		set
		{
			Headers.SetList("Set-Cookie", value);
		}
	}

	public ResponseHeaders(IHeaderDictionary headers)
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
