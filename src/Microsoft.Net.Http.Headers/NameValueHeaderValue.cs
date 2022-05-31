using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class NameValueHeaderValue
{
	private static readonly HttpHeaderParser<NameValueHeaderValue> SingleValueParser = new GenericHeaderParser<NameValueHeaderValue>(supportsMultipleValues: false, GetNameValueLength);

	internal static readonly HttpHeaderParser<NameValueHeaderValue> MultipleValueParser = new GenericHeaderParser<NameValueHeaderValue>(supportsMultipleValues: true, GetNameValueLength);

	private StringSegment _name;

	private StringSegment _value;

	private bool _isReadOnly;

	public StringSegment Name => _name;

	public StringSegment Value
	{
		get
		{
			return _value;
		}
		set
		{
			HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
			CheckValueFormat(value);
			_value = value;
		}
	}

	public bool IsReadOnly => _isReadOnly;

	private NameValueHeaderValue()
	{
	}

	public NameValueHeaderValue(StringSegment name)
		: this(name, null)
	{
	}

	public NameValueHeaderValue(StringSegment name, StringSegment value)
	{
		CheckNameValueFormat(name, value);
		_name = name;
		_value = value;
	}

	public NameValueHeaderValue Copy()
	{
		return new NameValueHeaderValue
		{
			_name = _name,
			_value = _value
		};
	}

	public NameValueHeaderValue CopyAsReadOnly()
	{
		if (IsReadOnly)
		{
			return this;
		}
		return new NameValueHeaderValue
		{
			_name = _name,
			_value = _value,
			_isReadOnly = true
		};
	}

	public override int GetHashCode()
	{
		int hashCode = StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_name);
		if (!StringSegment.IsNullOrEmpty(_value))
		{
			if (_value[0] == '"')
			{
				return hashCode ^ _value.GetHashCode();
			}
			return hashCode ^ StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_value);
		}
		return hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NameValueHeaderValue nameValueHeaderValue))
		{
			return false;
		}
		if (!_name.Equals(nameValueHeaderValue._name, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (StringSegment.IsNullOrEmpty(_value))
		{
			return StringSegment.IsNullOrEmpty(nameValueHeaderValue._value);
		}
		if (_value[0] == '"')
		{
			return _value.Equals(nameValueHeaderValue._value, StringComparison.Ordinal);
		}
		return _value.Equals(nameValueHeaderValue._value, StringComparison.OrdinalIgnoreCase);
	}

	public StringSegment GetUnescapedValue()
	{
		if (!HeaderUtilities.IsQuoted(_value))
		{
			return _value;
		}
		return HeaderUtilities.UnescapeAsQuotedString(_value);
	}

	public void SetAndEscapeValue(StringSegment value)
	{
		HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
		if (StringSegment.IsNullOrEmpty(value) || GetValueLength(value, 0) == value.Length)
		{
			_value = value;
		}
		else
		{
			Value = HeaderUtilities.EscapeAsQuotedString(value);
		}
	}

	public static NameValueHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return SingleValueParser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out NameValueHeaderValue parsedValue)
	{
		int index = 0;
		return SingleValueParser.TryParseValue(input, ref index, out parsedValue);
	}

	public static IList<NameValueHeaderValue> ParseList(IList<string> input)
	{
		return MultipleValueParser.ParseValues(input);
	}

	public static IList<NameValueHeaderValue> ParseStrictList(IList<string> input)
	{
		return MultipleValueParser.ParseStrictValues(input);
	}

	public static bool TryParseList(IList<string> input, out IList<NameValueHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseValues(input, out parsedValues);
	}

	public static bool TryParseStrictList(IList<string> input, out IList<NameValueHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseStrictValues(input, out parsedValues);
	}

	public override string ToString()
	{
		if (!StringSegment.IsNullOrEmpty(_value))
		{
			return string.Concat((object?)_name, "=", _value);
		}
		return _name.ToString();
	}

	internal static void ToString(IList<NameValueHeaderValue> values, char separator, bool leadingSeparator, StringBuilder destination)
	{
		if (values == null || values.Count == 0)
		{
			return;
		}
		for (int i = 0; i < values.Count; i++)
		{
			if (leadingSeparator || destination.Length > 0)
			{
				destination.Append(separator);
				destination.Append(' ');
			}
			destination.Append((object)values[i].Name);
			if (!StringSegment.IsNullOrEmpty(values[i].Value))
			{
				destination.Append('=');
				destination.Append((object)values[i].Value);
			}
		}
	}

	internal static string ToString(IList<NameValueHeaderValue> values, char separator, bool leadingSeparator)
	{
		if (values == null || values.Count == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		ToString(values, separator, leadingSeparator, stringBuilder);
		return stringBuilder.ToString();
	}

	internal static int GetHashCode(IList<NameValueHeaderValue> values)
	{
		if (values == null || values.Count == 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < values.Count; i++)
		{
			num ^= values[i].GetHashCode();
		}
		return num;
	}

	private static int GetNameValueLength(StringSegment input, int startIndex, out NameValueHeaderValue parsedValue)
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
		StringSegment name = input.Subsegment(startIndex, tokenLength);
		int num = startIndex + tokenLength;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (num == input.Length || input[num] != '=')
		{
			parsedValue = new NameValueHeaderValue();
			parsedValue._name = name;
			num += HttpRuleParser.GetWhitespaceLength(input, num);
			return num - startIndex;
		}
		num++;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		int valueLength = GetValueLength(input, num);
		parsedValue = new NameValueHeaderValue();
		parsedValue._name = name;
		parsedValue._value = input.Subsegment(num, valueLength);
		num += valueLength;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		return num - startIndex;
	}

	internal static int GetNameValueListLength(StringSegment input, int startIndex, char delimiter, IList<NameValueHeaderValue> nameValueCollection)
	{
		if (StringSegment.IsNullOrEmpty(input) || startIndex >= input.Length)
		{
			return 0;
		}
		int num = startIndex + HttpRuleParser.GetWhitespaceLength(input, startIndex);
		while (true)
		{
			NameValueHeaderValue parsedValue = null;
			int nameValueLength = GetNameValueLength(input, num, out parsedValue);
			if (nameValueLength == 0)
			{
				return num - startIndex;
			}
			nameValueCollection.Add(parsedValue);
			num += nameValueLength;
			num += HttpRuleParser.GetWhitespaceLength(input, num);
			if (num == input.Length || input[num] != delimiter)
			{
				break;
			}
			num++;
			num += HttpRuleParser.GetWhitespaceLength(input, num);
		}
		return num - startIndex;
	}

	public static NameValueHeaderValue Find(IList<NameValueHeaderValue> values, StringSegment name)
	{
		if (values == null || values.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < values.Count; i++)
		{
			NameValueHeaderValue nameValueHeaderValue = values[i];
			if (nameValueHeaderValue.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return nameValueHeaderValue;
			}
		}
		return null;
	}

	internal static int GetValueLength(StringSegment input, int startIndex)
	{
		if (startIndex >= input.Length)
		{
			return 0;
		}
		int length = HttpRuleParser.GetTokenLength(input, startIndex);
		if (length == 0 && HttpRuleParser.GetQuotedStringLength(input, startIndex, out length) != 0)
		{
			return 0;
		}
		return length;
	}

	private static void CheckNameValueFormat(StringSegment name, StringSegment value)
	{
		HeaderUtilities.CheckValidToken(name, "name");
		CheckValueFormat(value);
	}

	private static void CheckValueFormat(StringSegment value)
	{
		if (!StringSegment.IsNullOrEmpty(value) && GetValueLength(value, 0) != value.Length)
		{
			throw new FormatException(string.Format(CultureInfo.InvariantCulture, "The header value is invalid: '{0}'", value));
		}
	}

	private static NameValueHeaderValue CreateNameValue()
	{
		return new NameValueHeaderValue();
	}
}
