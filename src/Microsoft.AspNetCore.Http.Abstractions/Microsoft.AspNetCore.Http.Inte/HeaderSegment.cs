using System;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http.Internal;

public struct HeaderSegment : IEquatable<HeaderSegment>
{
	private readonly StringSegment _formatting;

	private readonly StringSegment _data;

	public StringSegment Formatting => _formatting;

	public StringSegment Data => _data;

	public HeaderSegment(StringSegment formatting, StringSegment data)
	{
		_formatting = formatting;
		_data = data;
	}

	public bool Equals(HeaderSegment other)
	{
		if (_formatting.Equals(other._formatting))
		{
			return _data.Equals(other._data);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is HeaderSegment)
		{
			return Equals((HeaderSegment)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (_formatting.GetHashCode() * 397) ^ _data.GetHashCode();
	}

	public static bool operator ==(HeaderSegment left, HeaderSegment right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(HeaderSegment left, HeaderSegment right)
	{
		return !left.Equals(right);
	}
}
