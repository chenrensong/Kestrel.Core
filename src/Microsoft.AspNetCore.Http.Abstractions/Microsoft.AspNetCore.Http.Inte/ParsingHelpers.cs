using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http.Internal;

public static class ParsingHelpers
{
	public static StringValues GetHeader(IHeaderDictionary headers, string key)
	{
		if (!headers.TryGetValue(key, out var value))
		{
			return StringValues.Empty;
		}
		return value;
	}

	public static StringValues GetHeaderSplit(IHeaderDictionary headers, string key)
	{
		return new StringValues(GetHeaderSplitImplementation(GetHeaderUnmodified(headers, key)).ToArray());
	}

	private static IEnumerable<string> GetHeaderSplitImplementation(StringValues values)
	{
		foreach (HeaderSegment item in new HeaderSegmentCollection(values))
		{
			if (!StringSegment.IsNullOrEmpty(item.Data))
			{
				string text = DeQuote(item.Data.Value);
				if (!string.IsNullOrEmpty(text))
				{
					yield return text;
				}
			}
		}
	}

	public static StringValues GetHeaderUnmodified(IHeaderDictionary headers, string key)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (!headers.TryGetValue(key, out var value))
		{
			return StringValues.Empty;
		}
		return value;
	}

	public static void SetHeaderJoined(IHeaderDictionary headers, string key, StringValues value)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		if (StringValues.IsNullOrEmpty(value))
		{
			headers.Remove(key);
			return;
		}
		headers[key] = string.Join(",", value.Select((string s) => QuoteIfNeeded(s)));
	}

	private static string QuoteIfNeeded(string value)
	{
		if (!string.IsNullOrEmpty(value) && Enumerable.Contains(value, ',') && (value[0] != '"' || value[value.Length - 1] != '"'))
		{
			return "\"" + value + "\"";
		}
		return value;
	}

	private static string DeQuote(string value)
	{
		if (!string.IsNullOrEmpty(value) && value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"')
		{
			value = value.Substring(1, value.Length - 2);
		}
		return value;
	}

	public static void SetHeaderUnmodified(IHeaderDictionary headers, string key, StringValues? values)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		if (!values.HasValue || StringValues.IsNullOrEmpty(values.Value))
		{
			headers.Remove(key);
		}
		else
		{
			headers[key] = values.Value;
		}
	}

	public static void AppendHeaderJoined(IHeaderDictionary headers, string key, params string[] values)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (values == null || values.Length == 0)
		{
			return;
		}
		string text = GetHeader(headers, key);
		if (text == null)
		{
			SetHeaderJoined(headers, key, values);
			return;
		}
		headers[key] = text + "," + string.Join(",", values.Select((string value) => QuoteIfNeeded(value)));
	}

	public static void AppendHeaderUnmodified(IHeaderDictionary headers, string key, StringValues values)
	{
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (values.Count != 0)
		{
			StringValues headerUnmodified = GetHeaderUnmodified(headers, key);
			SetHeaderUnmodified(headers, key, StringValues.Concat(headerUnmodified, values));
		}
	}
}
