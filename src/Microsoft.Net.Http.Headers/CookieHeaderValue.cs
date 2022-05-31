using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class CookieHeaderValue
{
	private static readonly CookieHeaderParser SingleValueParser = new CookieHeaderParser(supportsMultipleValues: false);

	private static readonly CookieHeaderParser MultipleValueParser = new CookieHeaderParser(supportsMultipleValues: true);

	private StringSegment _name;

	private StringSegment _value;

	public StringSegment Name
	{
		get
		{
			return _name;
		}
		set
		{
			CheckNameFormat(value, "value");
			_name = value;
		}
	}

	public StringSegment Value
	{
		get
		{
			return _value;
		}
		set
		{
			CheckValueFormat(value, "value");
			_value = value;
		}
	}

	private CookieHeaderValue()
	{
	}

	public CookieHeaderValue(StringSegment name)
		: this(name, StringSegment.Empty)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
	}

	public CookieHeaderValue(StringSegment name, StringSegment value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Name = name;
		Value = value;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((object)_name);
		stringBuilder.Append("=");
		stringBuilder.Append((object)_value);
		return stringBuilder.ToString();
	}

	public static CookieHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return SingleValueParser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out CookieHeaderValue parsedValue)
	{
		int index = 0;
		return SingleValueParser.TryParseValue(input, ref index, out parsedValue);
	}

	public static IList<CookieHeaderValue> ParseList(IList<string> inputs)
	{
		return MultipleValueParser.ParseValues(inputs);
	}

	public static IList<CookieHeaderValue> ParseStrictList(IList<string> inputs)
	{
		return MultipleValueParser.ParseStrictValues(inputs);
	}

	public static bool TryParseList(IList<string> inputs, out IList<CookieHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseValues(inputs, out parsedValues);
	}

	public static bool TryParseStrictList(IList<string> inputs, out IList<CookieHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseStrictValues(inputs, out parsedValues);
	}

	internal static bool TryGetCookieLength(StringSegment input, ref int offset, out CookieHeaderValue parsedValue)
	{
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(input) || offset >= input.Length)
		{
			return false;
		}
		CookieHeaderValue cookieHeaderValue = new CookieHeaderValue();
		int tokenLength = HttpRuleParser.GetTokenLength(input, offset);
		if (tokenLength == 0)
		{
			return false;
		}
		cookieHeaderValue._name = input.Subsegment(offset, tokenLength);
		offset += tokenLength;
		if (!ReadEqualsSign(input, ref offset))
		{
			return false;
		}
		cookieHeaderValue._value = GetCookieValue(input, ref offset);
		parsedValue = cookieHeaderValue;
		return true;
	}

	internal static StringSegment GetCookieValue(StringSegment input, ref int offset)
	{
		int num = offset;
		if (offset >= input.Length)
		{
			return StringSegment.Empty;
		}
		bool flag = false;
		if (input[offset] == '"')
		{
			flag = true;
			offset++;
		}
		while (offset < input.Length && IsCookieValueChar(input[offset]))
		{
			offset++;
		}
		if (flag)
		{
			if (offset == input.Length || input[offset] != '"')
			{
				return StringSegment.Empty;
			}
			offset++;
		}
		int length = offset - num;
		if (offset > num)
		{
			return input.Subsegment(num, length);
		}
		return StringSegment.Empty;
	}

	private static bool ReadEqualsSign(StringSegment input, ref int offset)
	{
		if (offset >= input.Length || input[offset] != '=')
		{
			return false;
		}
		offset++;
		return true;
	}

	private static bool IsCookieValueChar(char c)
	{
		if (c < '!' || c > '~')
		{
			return false;
		}
		if (c != '"' && c != ',' && c != ';')
		{
			return c != '\\';
		}
		return false;
	}

	internal static void CheckNameFormat(StringSegment name, string parameterName)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (HttpRuleParser.GetTokenLength(name, 0) != name.Length)
		{
			throw new ArgumentException("Invalid cookie name: " + name, parameterName);
		}
	}

	internal static void CheckValueFormat(StringSegment value, string parameterName)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int offset = 0;
		if (GetCookieValue(value, ref offset).Length != value.Length)
		{
			throw new ArgumentException("Invalid cookie value: " + value, parameterName);
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CookieHeaderValue cookieHeaderValue))
		{
			return false;
		}
		if (StringSegment.Equals(_name, cookieHeaderValue._name, StringComparison.OrdinalIgnoreCase))
		{
			return StringSegment.Equals(_value, cookieHeaderValue._value, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _name.GetHashCode() ^ _value.GetHashCode();
	}
}
