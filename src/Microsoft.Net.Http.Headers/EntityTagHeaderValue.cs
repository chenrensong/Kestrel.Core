using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class EntityTagHeaderValue
{
	private static readonly HttpHeaderParser<EntityTagHeaderValue> SingleValueParser = new GenericHeaderParser<EntityTagHeaderValue>(supportsMultipleValues: false, GetEntityTagLength);

	private static readonly HttpHeaderParser<EntityTagHeaderValue> MultipleValueParser = new GenericHeaderParser<EntityTagHeaderValue>(supportsMultipleValues: true, GetEntityTagLength);

	private static EntityTagHeaderValue AnyType;

	private StringSegment _tag;

	private bool _isWeak;

	public static EntityTagHeaderValue Any
	{
		get
		{
			if (AnyType == null)
			{
				AnyType = new EntityTagHeaderValue();
				AnyType._tag = "*";
				AnyType._isWeak = false;
			}
			return AnyType;
		}
	}

	public StringSegment Tag => _tag;

	public bool IsWeak => _isWeak;

	private EntityTagHeaderValue()
	{
	}

	public EntityTagHeaderValue(StringSegment tag)
		: this(tag, isWeak: false)
	{
	}

	public EntityTagHeaderValue(StringSegment tag, bool isWeak)
	{
		if (StringSegment.IsNullOrEmpty(tag))
		{
			throw new ArgumentException("An empty string is not allowed.", "tag");
		}
		int length = 0;
		if (!isWeak && StringSegment.Equals(tag, "*", StringComparison.Ordinal))
		{
			_tag = tag;
		}
		else if (HttpRuleParser.GetQuotedStringLength(tag, 0, out length) != 0 || length != tag.Length)
		{
			throw new FormatException("Invalid ETag name");
		}
		_tag = tag;
		_isWeak = isWeak;
	}

	public override string ToString()
	{
		if (_isWeak)
		{
			return "W/" + _tag;
		}
		return _tag.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is EntityTagHeaderValue entityTagHeaderValue))
		{
			return false;
		}
		if (_isWeak == entityTagHeaderValue._isWeak)
		{
			return StringSegment.Equals(_tag, entityTagHeaderValue._tag, StringComparison.Ordinal);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _tag.GetHashCode() ^ _isWeak.GetHashCode();
	}

	public bool Compare(EntityTagHeaderValue other, bool useStrongComparison)
	{
		if (other == null)
		{
			return false;
		}
		if (useStrongComparison)
		{
			if (!IsWeak && !other.IsWeak)
			{
				return StringSegment.Equals(Tag, other.Tag, StringComparison.Ordinal);
			}
			return false;
		}
		return StringSegment.Equals(Tag, other.Tag, StringComparison.Ordinal);
	}

	public static EntityTagHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return SingleValueParser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out EntityTagHeaderValue parsedValue)
	{
		int index = 0;
		return SingleValueParser.TryParseValue(input, ref index, out parsedValue);
	}

	public static IList<EntityTagHeaderValue> ParseList(IList<string> inputs)
	{
		return MultipleValueParser.ParseValues(inputs);
	}

	public static IList<EntityTagHeaderValue> ParseStrictList(IList<string> inputs)
	{
		return MultipleValueParser.ParseStrictValues(inputs);
	}

	public static bool TryParseList(IList<string> inputs, out IList<EntityTagHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseValues(inputs, out parsedValues);
	}

	public static bool TryParseStrictList(IList<string> inputs, out IList<EntityTagHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseStrictValues(inputs, out parsedValues);
	}

	internal static int GetEntityTagLength(StringSegment input, int startIndex, out EntityTagHeaderValue parsedValue)
	{
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(input) || startIndex >= input.Length)
		{
			return 0;
		}
		bool isWeak = false;
		int num = startIndex;
		char c = input[startIndex];
		if (c == '*')
		{
			parsedValue = Any;
			num++;
		}
		else
		{
			if (c == 'W' || c == 'w')
			{
				num++;
				if (num + 2 >= input.Length || input[num] != '/')
				{
					return 0;
				}
				isWeak = true;
				num++;
				num += HttpRuleParser.GetWhitespaceLength(input, num);
			}
			int offset = num;
			int length = 0;
			if (HttpRuleParser.GetQuotedStringLength(input, num, out length) != 0)
			{
				return 0;
			}
			parsedValue = new EntityTagHeaderValue();
			if (length == input.Length)
			{
				parsedValue._tag = input;
				parsedValue._isWeak = false;
			}
			else
			{
				parsedValue._tag = input.Subsegment(offset, length);
				parsedValue._isWeak = isWeak;
			}
			num += length;
		}
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		return num - startIndex;
	}
}
