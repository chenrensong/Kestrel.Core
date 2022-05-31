using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class RangeHeaderValue
{
	private static readonly HttpHeaderParser<RangeHeaderValue> Parser = new GenericHeaderParser<RangeHeaderValue>(supportsMultipleValues: false, GetRangeLength);

	private StringSegment _unit;

	private ICollection<RangeItemHeaderValue> _ranges;

	public StringSegment Unit
	{
		get
		{
			return _unit;
		}
		set
		{
			HeaderUtilities.CheckValidToken(value, "value");
			_unit = value;
		}
	}

	public ICollection<RangeItemHeaderValue> Ranges
	{
		get
		{
			if (_ranges == null)
			{
				_ranges = new ObjectCollection<RangeItemHeaderValue>();
			}
			return _ranges;
		}
	}

	public RangeHeaderValue()
	{
		_unit = "bytes";
	}

	public RangeHeaderValue(long? from, long? to)
	{
		_unit = "bytes";
		Ranges.Add(new RangeItemHeaderValue(from, to));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((object)_unit);
		stringBuilder.Append('=');
		bool flag = true;
		foreach (RangeItemHeaderValue range in Ranges)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(range.From);
			stringBuilder.Append('-');
			stringBuilder.Append(range.To);
		}
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RangeHeaderValue rangeHeaderValue))
		{
			return false;
		}
		if (StringSegment.Equals(_unit, rangeHeaderValue._unit, StringComparison.OrdinalIgnoreCase))
		{
			return HeaderUtilities.AreEqualCollections(Ranges, rangeHeaderValue.Ranges);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_unit);
		foreach (RangeItemHeaderValue range in Ranges)
		{
			num ^= range.GetHashCode();
		}
		return num;
	}

	public static RangeHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return Parser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out RangeHeaderValue parsedValue)
	{
		int index = 0;
		return Parser.TryParseValue(input, ref index, out parsedValue);
	}

	private static int GetRangeLength(StringSegment input, int startIndex, out RangeHeaderValue parsedValue)
	{
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(input) || startIndex >= input.Length)
		{
			return 0;
		}
		int tokenLength = HttpRuleParser.GetTokenLength(input, startIndex);
		if (tokenLength == 0)
		{
			return 0;
		}
		RangeHeaderValue rangeHeaderValue = new RangeHeaderValue();
		rangeHeaderValue._unit = input.Subsegment(startIndex, tokenLength);
		int num = startIndex + tokenLength;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (num == input.Length || input[num] != '=')
		{
			return 0;
		}
		num++;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		int rangeItemListLength = RangeItemHeaderValue.GetRangeItemListLength(input, num, rangeHeaderValue.Ranges);
		if (rangeItemListLength == 0)
		{
			return 0;
		}
		num += rangeItemListLength;
		parsedValue = rangeHeaderValue;
		return num - startIndex;
	}
}
