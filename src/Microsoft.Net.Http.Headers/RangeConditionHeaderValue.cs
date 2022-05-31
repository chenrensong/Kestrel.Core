using System;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class RangeConditionHeaderValue
{
	private static readonly HttpHeaderParser<RangeConditionHeaderValue> Parser = new GenericHeaderParser<RangeConditionHeaderValue>(supportsMultipleValues: false, GetRangeConditionLength);

	private DateTimeOffset? _lastModified;

	private EntityTagHeaderValue _entityTag;

	public DateTimeOffset? LastModified => _lastModified;

	public EntityTagHeaderValue EntityTag => _entityTag;

	private RangeConditionHeaderValue()
	{
	}

	public RangeConditionHeaderValue(DateTimeOffset lastModified)
	{
		_lastModified = lastModified;
	}

	public RangeConditionHeaderValue(EntityTagHeaderValue entityTag)
	{
		if (entityTag == null)
		{
			throw new ArgumentNullException("entityTag");
		}
		_entityTag = entityTag;
	}

	public RangeConditionHeaderValue(string entityTag)
		: this(new EntityTagHeaderValue(entityTag))
	{
	}

	public override string ToString()
	{
		if (_entityTag == null)
		{
			return HeaderUtilities.FormatDate(_lastModified.Value);
		}
		return _entityTag.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RangeConditionHeaderValue rangeConditionHeaderValue))
		{
			return false;
		}
		if (_entityTag == null)
		{
			if (rangeConditionHeaderValue._lastModified.HasValue)
			{
				return _lastModified.Value == rangeConditionHeaderValue._lastModified.Value;
			}
			return false;
		}
		return _entityTag.Equals(rangeConditionHeaderValue._entityTag);
	}

	public override int GetHashCode()
	{
		if (_entityTag == null)
		{
			return _lastModified.Value.GetHashCode();
		}
		return _entityTag.GetHashCode();
	}

	public static RangeConditionHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return Parser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out RangeConditionHeaderValue parsedValue)
	{
		int index = 0;
		return Parser.TryParseValue(input, ref index, out parsedValue);
	}

	private static int GetRangeConditionLength(StringSegment input, int startIndex, out RangeConditionHeaderValue parsedValue)
	{
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(input) || startIndex + 1 >= input.Length)
		{
			return 0;
		}
		int num = startIndex;
		DateTimeOffset result = DateTimeOffset.MinValue;
		EntityTagHeaderValue parsedValue2 = null;
		char c = input[num];
		char c2 = input[num + 1];
		if (c == '"' || ((c == 'w' || c == 'W') && c2 == '/'))
		{
			int entityTagLength = EntityTagHeaderValue.GetEntityTagLength(input, num, out parsedValue2);
			if (entityTagLength == 0)
			{
				return 0;
			}
			num += entityTagLength;
			if (num != input.Length)
			{
				return 0;
			}
		}
		else
		{
			if (!HttpRuleParser.TryStringToDate(input.Subsegment(num), out result))
			{
				return 0;
			}
			num = input.Length;
		}
		parsedValue = new RangeConditionHeaderValue();
		if (parsedValue2 == null)
		{
			parsedValue._lastModified = result;
		}
		else
		{
			parsedValue._entityTag = parsedValue2;
		}
		return num - startIndex;
	}
}
