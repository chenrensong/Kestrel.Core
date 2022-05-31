using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http;

public struct QueryString : IEquatable<QueryString>
{
	public static readonly QueryString Empty = new QueryString(string.Empty);

	private readonly string _value;

	public string Value => _value;

	public bool HasValue => !string.IsNullOrEmpty(_value);

	public QueryString(string value)
	{
		if (!string.IsNullOrEmpty(value) && value[0] != '?')
		{
			throw new ArgumentException("The leading '?' must be included for a non-empty query.", "value");
		}
		_value = value;
	}

	public override string ToString()
	{
		return ToUriComponent();
	}

	public string ToUriComponent()
	{
		if (!HasValue)
		{
			return string.Empty;
		}
		return _value.Replace("#", "%23");
	}

	public static QueryString FromUriComponent(string uriComponent)
	{
		if (string.IsNullOrEmpty(uriComponent))
		{
			return new QueryString(string.Empty);
		}
		return new QueryString(uriComponent);
	}

	public static QueryString FromUriComponent(Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		string text = uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
		if (!string.IsNullOrEmpty(text))
		{
			text = "?" + text;
		}
		return new QueryString(text);
	}

	public static QueryString Create(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!string.IsNullOrEmpty(value))
		{
			value = UrlEncoder.Default.Encode(value);
		}
		return new QueryString("?" + UrlEncoder.Default.Encode(name) + "=" + value);
	}

	public static QueryString Create(IEnumerable<KeyValuePair<string, string>> parameters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool first = true;
		foreach (KeyValuePair<string, string> parameter in parameters)
		{
			AppendKeyValuePair(stringBuilder, parameter.Key, parameter.Value, first);
			first = false;
		}
		return new QueryString(stringBuilder.ToString());
	}

	public static QueryString Create(IEnumerable<KeyValuePair<string, StringValues>> parameters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool first = true;
		foreach (KeyValuePair<string, StringValues> parameter in parameters)
		{
			if (StringValues.IsNullOrEmpty(parameter.Value))
			{
				AppendKeyValuePair(stringBuilder, parameter.Key, null, first);
				first = false;
				continue;
			}
			foreach (string item in parameter.Value)
			{
				AppendKeyValuePair(stringBuilder, parameter.Key, item, first);
				first = false;
			}
		}
		return new QueryString(stringBuilder.ToString());
	}

	public QueryString Add(QueryString other)
	{
		if (!HasValue || Value.Equals("?", StringComparison.Ordinal))
		{
			return other;
		}
		if (!other.HasValue || other.Value.Equals("?", StringComparison.Ordinal))
		{
			return this;
		}
		return new QueryString(_value + "&" + other.Value.Substring(1));
	}

	public QueryString Add(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!HasValue || Value.Equals("?", StringComparison.Ordinal))
		{
			return Create(name, value);
		}
		StringBuilder stringBuilder = new StringBuilder(Value);
		AppendKeyValuePair(stringBuilder, name, value, first: false);
		return new QueryString(stringBuilder.ToString());
	}

	public bool Equals(QueryString other)
	{
		if (!HasValue && !other.HasValue)
		{
			return true;
		}
		return string.Equals(_value, other._value, StringComparison.Ordinal);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return !HasValue;
		}
		if (obj is QueryString)
		{
			return Equals((QueryString)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (!HasValue)
		{
			return 0;
		}
		return _value.GetHashCode();
	}

	public static bool operator ==(QueryString left, QueryString right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(QueryString left, QueryString right)
	{
		return !left.Equals(right);
	}

	public static QueryString operator +(QueryString left, QueryString right)
	{
		return left.Add(right);
	}

	private static void AppendKeyValuePair(StringBuilder builder, string key, string value, bool first)
	{
		builder.Append(first ? "?" : "&");
		builder.Append(UrlEncoder.Default.Encode(key));
		builder.Append("=");
		if (!string.IsNullOrEmpty(value))
		{
			builder.Append(UrlEncoder.Default.Encode(value));
		}
	}
}
