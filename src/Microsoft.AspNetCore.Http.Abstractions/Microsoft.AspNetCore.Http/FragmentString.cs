using System;

namespace Microsoft.AspNetCore.Http;

public struct FragmentString : IEquatable<FragmentString>
{
	public static readonly FragmentString Empty = new FragmentString(string.Empty);

	private readonly string _value;

	public string Value => _value;

	public bool HasValue => !string.IsNullOrEmpty(_value);

	public FragmentString(string value)
	{
		if (!string.IsNullOrEmpty(value) && value[0] != '#')
		{
			throw new ArgumentException("The leading '#' must be included for a non-empty fragment.", "value");
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
		return _value;
	}

	public static FragmentString FromUriComponent(string uriComponent)
	{
		if (string.IsNullOrEmpty(uriComponent))
		{
			return Empty;
		}
		return new FragmentString(uriComponent);
	}

	public static FragmentString FromUriComponent(Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		string text = uri.GetComponents(UriComponents.Fragment, UriFormat.UriEscaped);
		if (!string.IsNullOrEmpty(text))
		{
			text = "#" + text;
		}
		return new FragmentString(text);
	}

	public bool Equals(FragmentString other)
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
		if (obj is FragmentString)
		{
			return Equals((FragmentString)obj);
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

	public static bool operator ==(FragmentString left, FragmentString right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FragmentString left, FragmentString right)
	{
		return !left.Equals(right);
	}
}
