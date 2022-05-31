using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class StringWithQualityHeaderValue
{
	private static readonly HttpHeaderParser<StringWithQualityHeaderValue> SingleValueParser = new GenericHeaderParser<StringWithQualityHeaderValue>(supportsMultipleValues: false, GetStringWithQualityLength);

	private static readonly HttpHeaderParser<StringWithQualityHeaderValue> MultipleValueParser = new GenericHeaderParser<StringWithQualityHeaderValue>(supportsMultipleValues: true, GetStringWithQualityLength);

	private StringSegment _value;

	private double? _quality;

	public StringSegment Value => _value;

	public double? Quality => _quality;

	private StringWithQualityHeaderValue()
	{
	}

	public StringWithQualityHeaderValue(StringSegment value)
	{
		HeaderUtilities.CheckValidToken(value, "value");
		_value = value;
	}

	public StringWithQualityHeaderValue(StringSegment value, double quality)
	{
		HeaderUtilities.CheckValidToken(value, "value");
		if (quality < 0.0 || quality > 1.0)
		{
			throw new ArgumentOutOfRangeException("quality");
		}
		_value = value;
		_quality = quality;
	}

	public override string ToString()
	{
		if (_quality.HasValue)
		{
			return string.Concat((object?)_value, "; q=", _quality.Value.ToString("0.0##", NumberFormatInfo.InvariantInfo));
		}
		return _value.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StringWithQualityHeaderValue stringWithQualityHeaderValue))
		{
			return false;
		}
		if (!StringSegment.Equals(_value, stringWithQualityHeaderValue._value, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (_quality.HasValue)
		{
			if (stringWithQualityHeaderValue._quality.HasValue)
			{
				return _quality.Value == stringWithQualityHeaderValue._quality.Value;
			}
			return false;
		}
		return !stringWithQualityHeaderValue._quality.HasValue;
	}

	public override int GetHashCode()
	{
		int num = StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_value);
		if (_quality.HasValue)
		{
			num ^= _quality.Value.GetHashCode();
		}
		return num;
	}

	public static StringWithQualityHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return SingleValueParser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out StringWithQualityHeaderValue parsedValue)
	{
		int index = 0;
		return SingleValueParser.TryParseValue(input, ref index, out parsedValue);
	}

	public static IList<StringWithQualityHeaderValue> ParseList(IList<string> input)
	{
		return MultipleValueParser.ParseValues(input);
	}

	public static IList<StringWithQualityHeaderValue> ParseStrictList(IList<string> input)
	{
		return MultipleValueParser.ParseStrictValues(input);
	}

	public static bool TryParseList(IList<string> input, out IList<StringWithQualityHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseValues(input, out parsedValues);
	}

	public static bool TryParseStrictList(IList<string> input, out IList<StringWithQualityHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseStrictValues(input, out parsedValues);
	}

	private static int GetStringWithQualityLength(StringSegment input, int startIndex, out StringWithQualityHeaderValue parsedValue)
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
		StringWithQualityHeaderValue stringWithQualityHeaderValue = new StringWithQualityHeaderValue();
		stringWithQualityHeaderValue._value = input.Subsegment(startIndex, tokenLength);
		int num = startIndex + tokenLength;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (num == input.Length || input[num] != ';')
		{
			parsedValue = stringWithQualityHeaderValue;
			return num - startIndex;
		}
		num++;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (!TryReadQuality(input, stringWithQualityHeaderValue, ref num))
		{
			return 0;
		}
		parsedValue = stringWithQualityHeaderValue;
		return num - startIndex;
	}

	private static bool TryReadQuality(StringSegment input, StringWithQualityHeaderValue result, ref int index)
	{
		int num = index;
		if (num == input.Length || (input[num] != 'q' && input[num] != 'Q'))
		{
			return false;
		}
		num++;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (num == input.Length || input[num] != '=')
		{
			return false;
		}
		num++;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (num == input.Length)
		{
			return false;
		}
		if (!HeaderUtilities.TryParseQualityDouble(input, num, out var quality, out var length))
		{
			return false;
		}
		result._quality = quality;
		num += length;
		num = (index = num + HttpRuleParser.GetWhitespaceLength(input, num));
		return true;
	}
}
