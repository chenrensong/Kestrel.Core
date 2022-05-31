using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

internal class CookieHeaderParser : HttpHeaderParser<CookieHeaderValue>
{
	internal CookieHeaderParser(bool supportsMultipleValues)
		: base(supportsMultipleValues)
	{
	}

	public sealed override bool TryParseValue(StringSegment value, ref int index, out CookieHeaderValue parsedValue)
	{
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(value) || index == value.Length)
		{
			return base.SupportsMultipleValues;
		}
		bool separatorFound = false;
		int offset = GetNextNonEmptyOrWhitespaceIndex(value, index, base.SupportsMultipleValues, out separatorFound);
		if (separatorFound && !base.SupportsMultipleValues)
		{
			return false;
		}
		if (offset == value.Length)
		{
			if (base.SupportsMultipleValues)
			{
				index = offset;
			}
			return base.SupportsMultipleValues;
		}
		CookieHeaderValue parsedValue2 = null;
		if (!CookieHeaderValue.TryGetCookieLength(value, ref offset, out parsedValue2))
		{
			return false;
		}
		offset = GetNextNonEmptyOrWhitespaceIndex(value, offset, base.SupportsMultipleValues, out separatorFound);
		if ((separatorFound && !base.SupportsMultipleValues) || (!separatorFound && offset < value.Length))
		{
			return false;
		}
		index = offset;
		parsedValue = parsedValue2;
		return true;
	}

	private static int GetNextNonEmptyOrWhitespaceIndex(StringSegment input, int startIndex, bool skipEmptyValues, out bool separatorFound)
	{
		separatorFound = false;
		int num = startIndex + HttpRuleParser.GetWhitespaceLength(input, startIndex);
		if (num == input.Length || (input[num] != ',' && input[num] != ';'))
		{
			return num;
		}
		separatorFound = true;
		num++;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (skipEmptyValues)
		{
			while (num < input.Length && (input[num] == ',' || input[num] == ';'))
			{
				num++;
				num += HttpRuleParser.GetWhitespaceLength(input, num);
			}
		}
		return num;
	}
}
