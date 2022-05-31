using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http.Abstractions;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http;

public struct HostString : IEquatable<HostString>
{
	private readonly string _value;

	public string Value => _value;

	public bool HasValue => !string.IsNullOrEmpty(_value);

	public string Host
	{
		get
		{
			GetParts(_value, out var host, out var _);
			return host.ToString();
		}
	}

	public int? Port
	{
		get
		{
			GetParts(_value, out var _, out var port);
			if (!StringSegment.IsNullOrEmpty(port) && int.TryParse(port.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
			{
				return result;
			}
			return null;
		}
	}

	public HostString(string value)
	{
		_value = value;
	}

	public HostString(string host, int port)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (port <= 0)
		{
			throw new ArgumentOutOfRangeException("port", Resources.Exception_PortMustBeGreaterThanZero);
		}
		int num;
		if (host.IndexOf('[') == -1 && (num = host.IndexOf(':')) >= 0 && num < host.Length - 1 && host.IndexOf(':', num + 1) >= 0)
		{
			host = "[" + host + "]";
		}
		_value = host + ":" + port.ToString(CultureInfo.InvariantCulture);
	}

	public override string ToString()
	{
		return ToUriComponent();
	}

	public string ToUriComponent()
	{
		if (string.IsNullOrEmpty(_value))
		{
			return string.Empty;
		}
		int i;
		for (i = 0; i < _value.Length && HostStringHelper.IsSafeHostStringChar(_value[i]); i++)
		{
		}
		if (i != _value.Length)
		{
			GetParts(_value, out var host, out var port);
			string ascii = new IdnMapping().GetAscii(host.Buffer, host.Offset, host.Length);
			if (!StringSegment.IsNullOrEmpty(port))
			{
				return ascii + ":" + port;
			}
			return ascii;
		}
		return _value;
	}

	public static HostString FromUriComponent(string uriComponent)
	{
		int num;
		if (!string.IsNullOrEmpty(uriComponent) && uriComponent.IndexOf('[') < 0 && ((num = uriComponent.IndexOf(':')) < 0 || num >= uriComponent.Length - 1 || uriComponent.IndexOf(':', num + 1) < 0) && uriComponent.IndexOf("xn--", StringComparison.Ordinal) >= 0)
		{
			if (num >= 0)
			{
				string text = uriComponent.Substring(num);
				uriComponent = new IdnMapping().GetUnicode(uriComponent, 0, num) + text;
			}
			else
			{
				uriComponent = new IdnMapping().GetUnicode(uriComponent);
			}
		}
		return new HostString(uriComponent);
	}

	public static HostString FromUriComponent(Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		return new HostString(uri.GetComponents(UriComponents.HostAndPort | UriComponents.NormalizedHost, UriFormat.Unescaped));
	}

	public static bool MatchesAny(StringSegment value, IList<StringSegment> patterns)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (patterns == null)
		{
			throw new ArgumentNullException("patterns");
		}
		GetParts(value, out var host, out var port);
		for (int i = 0; i < port.Length; i++)
		{
			if (port[i] < '0' || '9' < port[i])
			{
				throw new FormatException($"The given host value '{value}' has a malformed port.");
			}
		}
		for (int j = 0; j < patterns.Count; j++)
		{
			StringSegment stringSegment = patterns[j];
			if (stringSegment == "*")
			{
				return true;
			}
			if (StringSegment.Equals(stringSegment, host, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (stringSegment.StartsWith("*.", StringComparison.Ordinal) && host.Length >= stringSegment.Length)
			{
				StringSegment other = stringSegment.Subsegment(1);
				if (host.Subsegment(host.Length - other.Length).Equals(other, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Equals(HostString other)
	{
		if (!HasValue && !other.HasValue)
		{
			return true;
		}
		return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return !HasValue;
		}
		if (obj is HostString)
		{
			return Equals((HostString)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (!HasValue)
		{
			return 0;
		}
		return StringComparer.OrdinalIgnoreCase.GetHashCode(_value);
	}

	public static bool operator ==(HostString left, HostString right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(HostString left, HostString right)
	{
		return !left.Equals(right);
	}

	private static void GetParts(StringSegment value, out StringSegment host, out StringSegment port)
	{
		port = null;
		host = null;
		if (StringSegment.IsNullOrEmpty(value))
		{
			return;
		}
		int num;
		if ((num = value.IndexOf(']')) >= 0)
		{
			host = value.Subsegment(0, num + 1);
			if (num + 2 < value.Length && value[num + 1] == ':')
			{
				port = value.Subsegment(num + 2);
			}
		}
		else if ((num = value.IndexOf(':')) >= 0 && num < value.Length - 1 && value.IndexOf(':', num + 1) >= 0)
		{
			host = $"[{value}]";
			port = null;
		}
		else if (num >= 0)
		{
			host = value.Subsegment(0, num);
			port = value.Subsegment(num + 1);
		}
		else
		{
			host = value;
			port = null;
		}
	}
}
