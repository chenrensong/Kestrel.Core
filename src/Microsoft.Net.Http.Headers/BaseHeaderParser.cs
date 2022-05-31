using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

internal abstract class BaseHeaderParser<T> : HttpHeaderParser<T>
{
	protected BaseHeaderParser(bool supportsMultipleValues)
		: base(supportsMultipleValues)
	{
	}

	protected abstract int GetParsedValueLength(StringSegment value, int startIndex, out T parsedValue);

	public sealed override bool TryParseValue(StringSegment value, ref int index, out T parsedValue)
	{
		parsedValue = default(T);
		if (StringSegment.IsNullOrEmpty(value) || index == value.Length)
		{
			return base.SupportsMultipleValues;
		}
		bool separatorFound = false;
		int nextNonEmptyOrWhitespaceIndex = HeaderUtilities.GetNextNonEmptyOrWhitespaceIndex(value, index, base.SupportsMultipleValues, out separatorFound);
		if (separatorFound && !base.SupportsMultipleValues)
		{
			return false;
		}
		if (nextNonEmptyOrWhitespaceIndex == value.Length)
		{
			if (base.SupportsMultipleValues)
			{
				index = nextNonEmptyOrWhitespaceIndex;
			}
			return base.SupportsMultipleValues;
		}
		T parsedValue2;
		int parsedValueLength = GetParsedValueLength(value, nextNonEmptyOrWhitespaceIndex, out parsedValue2);
		if (parsedValueLength == 0)
		{
			return false;
		}
		nextNonEmptyOrWhitespaceIndex += parsedValueLength;
		nextNonEmptyOrWhitespaceIndex = HeaderUtilities.GetNextNonEmptyOrWhitespaceIndex(value, nextNonEmptyOrWhitespaceIndex, base.SupportsMultipleValues, out separatorFound);
		if ((separatorFound && !base.SupportsMultipleValues) || (!separatorFound && nextNonEmptyOrWhitespaceIndex < value.Length))
		{
			return false;
		}
		index = nextNonEmptyOrWhitespaceIndex;
		parsedValue = parsedValue2;
		return true;
	}
}
