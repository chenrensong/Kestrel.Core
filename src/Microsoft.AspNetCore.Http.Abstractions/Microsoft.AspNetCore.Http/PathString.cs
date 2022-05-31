using System;
using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Http.Abstractions;
using Microsoft.AspNetCore.Http.Internal;

namespace Microsoft.AspNetCore.Http;

[TypeConverter(typeof(PathStringConverter))]
public struct PathString : IEquatable<PathString>
{
	private static readonly char[] splitChar = new char[1] { '/' };

	public static readonly PathString Empty = new PathString(string.Empty);

	private readonly string _value;

	public string Value => _value;

	public bool HasValue => !string.IsNullOrEmpty(_value);

	public PathString(string value)
	{
		if (!string.IsNullOrEmpty(value) && value[0] != '/')
		{
			throw new ArgumentException(Resources.FormatException_PathMustStartWithSlash("value"), "value");
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
		StringBuilder stringBuilder = null;
		int startIndex = 0;
		int num = 0;
		bool flag = false;
		int num2 = 0;
		while (num2 < _value.Length)
		{
			bool flag2 = PathStringHelper.IsPercentEncodedChar(_value, num2);
			if (PathStringHelper.IsValidPathChar(_value[num2]) || flag2)
			{
				if (flag)
				{
					if (stringBuilder == null)
					{
						stringBuilder = new StringBuilder(_value.Length * 3);
					}
					stringBuilder.Append(Uri.EscapeDataString(_value.Substring(startIndex, num)));
					flag = false;
					startIndex = num2;
					num = 0;
				}
				if (flag2)
				{
					num += 3;
					num2 += 3;
				}
				else
				{
					num++;
					num2++;
				}
				continue;
			}
			if (!flag)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(_value.Length * 3);
				}
				stringBuilder.Append(_value, startIndex, num);
				flag = true;
				startIndex = num2;
				num = 0;
			}
			num++;
			num2++;
		}
		if (num == _value.Length && !flag)
		{
			return _value;
		}
		if (num > 0)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(_value.Length * 3);
			}
			if (flag)
			{
				stringBuilder.Append(Uri.EscapeDataString(_value.Substring(startIndex, num)));
			}
			else
			{
				stringBuilder.Append(_value, startIndex, num);
			}
		}
		return stringBuilder.ToString();
	}

	public static PathString FromUriComponent(string uriComponent)
	{
		return new PathString(Uri.UnescapeDataString(uriComponent));
	}

	public static PathString FromUriComponent(Uri uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		return new PathString("/" + uri.GetComponents(UriComponents.Path, UriFormat.Unescaped));
	}

	public bool StartsWithSegments(PathString other)
	{
		return StartsWithSegments(other, StringComparison.OrdinalIgnoreCase);
	}

	public bool StartsWithSegments(PathString other, StringComparison comparisonType)
	{
		string text = Value ?? string.Empty;
		string text2 = other.Value ?? string.Empty;
		if (text.StartsWith(text2, comparisonType))
		{
			if (text.Length != text2.Length)
			{
				return text[text2.Length] == '/';
			}
			return true;
		}
		return false;
	}

	public bool StartsWithSegments(PathString other, out PathString remaining)
	{
		return StartsWithSegments(other, StringComparison.OrdinalIgnoreCase, out remaining);
	}

	public bool StartsWithSegments(PathString other, StringComparison comparisonType, out PathString remaining)
	{
		string text = Value ?? string.Empty;
		string text2 = other.Value ?? string.Empty;
		if (text.StartsWith(text2, comparisonType) && (text.Length == text2.Length || text[text2.Length] == '/'))
		{
			remaining = new PathString(text.Substring(text2.Length));
			return true;
		}
		remaining = Empty;
		return false;
	}

	public bool StartsWithSegments(PathString other, out PathString matched, out PathString remaining)
	{
		return StartsWithSegments(other, StringComparison.OrdinalIgnoreCase, out matched, out remaining);
	}

	public bool StartsWithSegments(PathString other, StringComparison comparisonType, out PathString matched, out PathString remaining)
	{
		string text = Value ?? string.Empty;
		string text2 = other.Value ?? string.Empty;
		if (text.StartsWith(text2, comparisonType) && (text.Length == text2.Length || text[text2.Length] == '/'))
		{
			matched = new PathString(text.Substring(0, text2.Length));
			remaining = new PathString(text.Substring(text2.Length));
			return true;
		}
		remaining = Empty;
		matched = Empty;
		return false;
	}

	public PathString Add(PathString other)
	{
		if (HasValue && other.HasValue && Value[Value.Length - 1] == '/')
		{
			return new PathString(Value + other.Value.Substring(1));
		}
		return new PathString(Value + other.Value);
	}

	public string Add(QueryString other)
	{
		return ToUriComponent() + other.ToUriComponent();
	}

	public bool Equals(PathString other)
	{
		return Equals(other, StringComparison.OrdinalIgnoreCase);
	}

	public bool Equals(PathString other, StringComparison comparisonType)
	{
		if (!HasValue && !other.HasValue)
		{
			return true;
		}
		return string.Equals(_value, other._value, comparisonType);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return !HasValue;
		}
		if (obj is PathString)
		{
			return Equals((PathString)obj);
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

	public static bool operator ==(PathString left, PathString right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PathString left, PathString right)
	{
		return !left.Equals(right);
	}

	public static string operator +(string left, PathString right)
	{
		return left + right.ToString();
	}

	public static string operator +(PathString left, string right)
	{
		return left.ToString() + right;
	}

	public static PathString operator +(PathString left, PathString right)
	{
		return left.Add(right);
	}

	public static string operator +(PathString left, QueryString right)
	{
		return left.Add(right);
	}

	public static implicit operator PathString(string s)
	{
		return ConvertFromString(s);
	}

	public static implicit operator string(PathString path)
	{
		return path.ToString();
	}

	internal static PathString ConvertFromString(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			return FromUriComponent(s);
		}
		return new PathString(s);
	}
}
