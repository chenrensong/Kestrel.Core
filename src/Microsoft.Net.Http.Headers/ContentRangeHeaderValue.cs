using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class ContentRangeHeaderValue
{
	private static readonly HttpHeaderParser<ContentRangeHeaderValue> Parser = new GenericHeaderParser<ContentRangeHeaderValue>(supportsMultipleValues: false, GetContentRangeLength);

	private StringSegment _unit;

	private long? _from;

	private long? _to;

	private long? _length;

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

	public long? From => _from;

	public long? To => _to;

	public long? Length => _length;

	public bool HasLength => _length.HasValue;

	public bool HasRange => _from.HasValue;

	private ContentRangeHeaderValue()
	{
	}

	public ContentRangeHeaderValue(long from, long to, long length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (to < 0 || to > length)
		{
			throw new ArgumentOutOfRangeException("to");
		}
		if (from < 0 || from > to)
		{
			throw new ArgumentOutOfRangeException("from");
		}
		_from = from;
		_to = to;
		_length = length;
		_unit = "bytes";
	}

	public ContentRangeHeaderValue(long length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		_length = length;
		_unit = "bytes";
	}

	public ContentRangeHeaderValue(long from, long to)
	{
		if (to < 0)
		{
			throw new ArgumentOutOfRangeException("to");
		}
		if (from < 0 || from > to)
		{
			throw new ArgumentOutOfRangeException("from");
		}
		_from = from;
		_to = to;
		_unit = "bytes";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ContentRangeHeaderValue contentRangeHeaderValue))
		{
			return false;
		}
		if (_from == contentRangeHeaderValue._from && _to == contentRangeHeaderValue._to && _length == contentRangeHeaderValue._length)
		{
			return StringSegment.Equals(_unit, contentRangeHeaderValue._unit, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_unit);
		if (HasRange)
		{
			num = num ^ _from.GetHashCode() ^ _to.GetHashCode();
		}
		if (HasLength)
		{
			num ^= _length.GetHashCode();
		}
		return num;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((object)_unit);
		stringBuilder.Append(' ');
		if (HasRange)
		{
			stringBuilder.Append(_from.Value.ToString(NumberFormatInfo.InvariantInfo));
			stringBuilder.Append('-');
			stringBuilder.Append(_to.Value.ToString(NumberFormatInfo.InvariantInfo));
		}
		else
		{
			stringBuilder.Append('*');
		}
		stringBuilder.Append('/');
		if (HasLength)
		{
			stringBuilder.Append(_length.Value.ToString(NumberFormatInfo.InvariantInfo));
		}
		else
		{
			stringBuilder.Append('*');
		}
		return stringBuilder.ToString();
	}

	public static ContentRangeHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return Parser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out ContentRangeHeaderValue parsedValue)
	{
		int index = 0;
		return Parser.TryParseValue(input, ref index, out parsedValue);
	}

	private static int GetContentRangeLength(StringSegment input, int startIndex, out ContentRangeHeaderValue parsedValue)
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
		StringSegment unit = input.Subsegment(startIndex, tokenLength);
		int num = startIndex + tokenLength;
		int whitespaceLength = HttpRuleParser.GetWhitespaceLength(input, num);
		if (whitespaceLength == 0)
		{
			return 0;
		}
		num += whitespaceLength;
		if (num == input.Length)
		{
			return 0;
		}
		int fromStartIndex = num;
		int fromLength = 0;
		int toStartIndex = 0;
		int toLength = 0;
		if (!TryGetRangeLength(input, ref num, out fromLength, out toStartIndex, out toLength))
		{
			return 0;
		}
		if (num == input.Length || input[num] != '/')
		{
			return 0;
		}
		num++;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (num == input.Length)
		{
			return 0;
		}
		int lengthStartIndex = num;
		int lengthLength = 0;
		if (!TryGetLengthLength(input, ref num, out lengthLength))
		{
			return 0;
		}
		if (!TryCreateContentRange(input, unit, fromStartIndex, fromLength, toStartIndex, toLength, lengthStartIndex, lengthLength, out parsedValue))
		{
			return 0;
		}
		return num - startIndex;
	}

	private static bool TryGetLengthLength(StringSegment input, ref int current, out int lengthLength)
	{
		lengthLength = 0;
		if (input[current] == '*')
		{
			current++;
		}
		else
		{
			lengthLength = HttpRuleParser.GetNumberLength(input, current, allowDecimal: false);
			if (lengthLength == 0 || lengthLength > 19)
			{
				return false;
			}
			current += lengthLength;
		}
		current += HttpRuleParser.GetWhitespaceLength(input, current);
		return true;
	}

	private static bool TryGetRangeLength(StringSegment input, ref int current, out int fromLength, out int toStartIndex, out int toLength)
	{
		fromLength = 0;
		toStartIndex = 0;
		toLength = 0;
		if (input[current] == '*')
		{
			current++;
		}
		else
		{
			fromLength = HttpRuleParser.GetNumberLength(input, current, allowDecimal: false);
			if (fromLength == 0 || fromLength > 19)
			{
				return false;
			}
			current += fromLength;
			current += HttpRuleParser.GetWhitespaceLength(input, current);
			if (current == input.Length || input[current] != '-')
			{
				return false;
			}
			current++;
			current += HttpRuleParser.GetWhitespaceLength(input, current);
			if (current == input.Length)
			{
				return false;
			}
			toStartIndex = current;
			toLength = HttpRuleParser.GetNumberLength(input, current, allowDecimal: false);
			if (toLength == 0 || toLength > 19)
			{
				return false;
			}
			current += toLength;
		}
		current += HttpRuleParser.GetWhitespaceLength(input, current);
		return true;
	}

	private static bool TryCreateContentRange(StringSegment input, StringSegment unit, int fromStartIndex, int fromLength, int toStartIndex, int toLength, int lengthStartIndex, int lengthLength, out ContentRangeHeaderValue parsedValue)
	{
		parsedValue = null;
		long result = 0L;
		if (fromLength > 0 && !HeaderUtilities.TryParseNonNegativeInt64(input.Subsegment(fromStartIndex, fromLength), out result))
		{
			return false;
		}
		long result2 = 0L;
		if (toLength > 0 && !HeaderUtilities.TryParseNonNegativeInt64(input.Subsegment(toStartIndex, toLength), out result2))
		{
			return false;
		}
		if (fromLength > 0 && toLength > 0 && result > result2)
		{
			return false;
		}
		long result3 = 0L;
		if (lengthLength > 0 && !HeaderUtilities.TryParseNonNegativeInt64(input.Subsegment(lengthStartIndex, lengthLength), out result3))
		{
			return false;
		}
		if (toLength > 0 && lengthLength > 0 && result2 >= result3)
		{
			return false;
		}
		ContentRangeHeaderValue contentRangeHeaderValue = new ContentRangeHeaderValue();
		contentRangeHeaderValue._unit = unit;
		if (fromLength > 0)
		{
			contentRangeHeaderValue._from = result;
			contentRangeHeaderValue._to = result2;
		}
		if (lengthLength > 0)
		{
			contentRangeHeaderValue._length = result3;
		}
		parsedValue = contentRangeHeaderValue;
		return true;
	}
}
