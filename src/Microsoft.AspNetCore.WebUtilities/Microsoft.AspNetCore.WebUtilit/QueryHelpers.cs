using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebUtilities;

public static class QueryHelpers
{
	public static string AddQueryString(string uri, string name, string value)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return AddQueryString(uri, new KeyValuePair<string, string>[1]
		{
			new KeyValuePair<string, string>(name, value)
		});
	}

	public static string AddQueryString(string uri, IDictionary<string, string> queryString)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (queryString == null)
		{
			throw new ArgumentNullException("queryString");
		}
		return AddQueryString(uri, (IEnumerable<KeyValuePair<string, string>>)queryString);
	}

	private static string AddQueryString(string uri, IEnumerable<KeyValuePair<string, string>> queryString)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (queryString == null)
		{
			throw new ArgumentNullException("queryString");
		}
		int num = uri.IndexOf('#');
		string text = uri;
		string value = "";
		if (num != -1)
		{
			value = uri.Substring(num);
			text = uri.Substring(0, num);
		}
		bool flag = text.IndexOf('?') != -1;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(text);
		foreach (KeyValuePair<string, string> item in queryString)
		{
			stringBuilder.Append(flag ? '&' : '?');
			stringBuilder.Append(UrlEncoder.Default.Encode(item.Key));
			stringBuilder.Append('=');
			stringBuilder.Append(UrlEncoder.Default.Encode(item.Value));
			flag = true;
		}
		stringBuilder.Append(value);
		return stringBuilder.ToString();
	}

	public static Dictionary<string, StringValues> ParseQuery(string queryString)
	{
		Dictionary<string, StringValues> dictionary = ParseNullableQuery(queryString);
		if (dictionary == null)
		{
			return new Dictionary<string, StringValues>();
		}
		return dictionary;
	}

	public static Dictionary<string, StringValues> ParseNullableQuery(string queryString)
	{
		KeyValueAccumulator keyValueAccumulator = default(KeyValueAccumulator);
		if (string.IsNullOrEmpty(queryString) || queryString == "?")
		{
			return null;
		}
		int i = 0;
		if (queryString[0] == '?')
		{
			i = 1;
		}
		int length = queryString.Length;
		int num = queryString.IndexOf('=');
		if (num == -1)
		{
			num = length;
		}
		while (i < length)
		{
			int num2 = queryString.IndexOf('&', i);
			if (num2 == -1)
			{
				num2 = length;
			}
			if (num < num2)
			{
				for (; i != num && char.IsWhiteSpace(queryString[i]); i++)
				{
				}
				string text = queryString.Substring(i, num - i);
				string text2 = queryString.Substring(num + 1, num2 - num - 1);
				keyValueAccumulator.Append(Uri.UnescapeDataString(text.Replace('+', ' ')), Uri.UnescapeDataString(text2.Replace('+', ' ')));
				num = queryString.IndexOf('=', num2);
				if (num == -1)
				{
					num = length;
				}
			}
			else if (num2 > i)
			{
				keyValueAccumulator.Append(queryString.Substring(i, num2 - i), string.Empty);
			}
			i = num2 + 1;
		}
		if (!keyValueAccumulator.HasValues)
		{
			return null;
		}
		return keyValueAccumulator.GetResults();
	}
}
