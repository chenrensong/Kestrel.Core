using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http.Internal;

public class ResponseCookies : IResponseCookies
{
	private IHeaderDictionary Headers { get; set; }

	public ResponseCookies(IHeaderDictionary headers, ObjectPool<StringBuilder> builderPool)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		Headers = headers;
	}

	public void Append(string key, string value)
	{
		string value2 = new SetCookieHeaderValue(Uri.EscapeDataString(key), Uri.EscapeDataString(value))
		{
			Path = "/"
		}.ToString();
		IHeaderDictionary headers = Headers;
		StringValues values = Headers["Set-Cookie"];
		headers["Set-Cookie"] = StringValues.Concat(in values, value2);
	}

	public void Append(string key, string value, CookieOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		string value2 = new SetCookieHeaderValue(Uri.EscapeDataString(key), Uri.EscapeDataString(value))
		{
			Domain = options.Domain,
			Path = options.Path,
			Expires = options.Expires,
			MaxAge = options.MaxAge,
			Secure = options.Secure,
			SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)options.SameSite,
			HttpOnly = options.HttpOnly
		}.ToString();
		IHeaderDictionary headers = Headers;
		StringValues values = Headers["Set-Cookie"];
		headers["Set-Cookie"] = StringValues.Concat(in values, value2);
	}

	public void Delete(string key)
	{
		Delete(key, new CookieOptions
		{
			Path = "/"
		});
	}

	public void Delete(string key, CookieOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		string arg = Uri.EscapeDataString(key) + "=";
		bool num = !string.IsNullOrEmpty(options.Domain);
		bool flag = !string.IsNullOrEmpty(options.Path);
		Func<string, string, CookieOptions, bool> func = (num ? ((Func<string, string, CookieOptions, bool>)((string value, string encKeyPlusEquals, CookieOptions opts) => value.StartsWith(encKeyPlusEquals, StringComparison.OrdinalIgnoreCase) && value.IndexOf("domain=" + opts.Domain, StringComparison.OrdinalIgnoreCase) != -1)) : ((!flag) ? ((Func<string, string, CookieOptions, bool>)((string value, string encKeyPlusEquals, CookieOptions opts) => value.StartsWith(encKeyPlusEquals, StringComparison.OrdinalIgnoreCase))) : ((Func<string, string, CookieOptions, bool>)((string value, string encKeyPlusEquals, CookieOptions opts) => value.StartsWith(encKeyPlusEquals, StringComparison.OrdinalIgnoreCase) && value.IndexOf("path=" + opts.Path, StringComparison.OrdinalIgnoreCase) != -1))));
		StringValues value2 = Headers["Set-Cookie"];
		if (!StringValues.IsNullOrEmpty(value2))
		{
			string[] array = value2.ToArray();
			List<string> list = new List<string>();
			for (int i = 0; i < array.Length; i++)
			{
				if (!func(array[i], arg, options))
				{
					list.Add(array[i]);
				}
			}
			Headers["Set-Cookie"] = new StringValues(list.ToArray());
		}
		Append(key, string.Empty, new CookieOptions
		{
			Path = options.Path,
			Domain = options.Domain,
			Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			Secure = options.Secure,
			HttpOnly = options.HttpOnly,
			SameSite = options.SameSite
		});
	}
}
