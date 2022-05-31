using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

internal abstract class HttpHeaderParser<T>
{
	private bool _supportsMultipleValues;

	public bool SupportsMultipleValues => _supportsMultipleValues;

	protected HttpHeaderParser(bool supportsMultipleValues)
	{
		_supportsMultipleValues = supportsMultipleValues;
	}

	public abstract bool TryParseValue(StringSegment value, ref int index, out T parsedValue);

	public T ParseValue(StringSegment value, ref int index)
	{
		if (!TryParseValue(value, ref index, out var parsedValue))
		{
			throw new FormatException(string.Format(CultureInfo.InvariantCulture, "The header contains invalid values at index {0}: '{1}'", index, value.Value ?? "<null>"));
		}
		return parsedValue;
	}

	public virtual bool TryParseValues(IList<string> values, out IList<T> parsedValues)
	{
		return TryParseValues(values, strict: false, out parsedValues);
	}

	public virtual bool TryParseStrictValues(IList<string> values, out IList<T> parsedValues)
	{
		return TryParseValues(values, strict: true, out parsedValues);
	}

	protected virtual bool TryParseValues(IList<string> values, bool strict, out IList<T> parsedValues)
	{
		parsedValues = null;
		List<T> list = null;
		if (values == null)
		{
			return false;
		}
		for (int i = 0; i < values.Count; i++)
		{
			string text = values[i];
			int index = 0;
			while (!string.IsNullOrEmpty(text) && index < text.Length)
			{
				if (TryParseValue(text, ref index, out var parsedValue))
				{
					if (parsedValue != null)
					{
						if (list == null)
						{
							list = new List<T>();
						}
						list.Add(parsedValue);
					}
				}
				else
				{
					if (strict)
					{
						return false;
					}
					index++;
				}
			}
		}
		if (list != null)
		{
			parsedValues = list;
			return true;
		}
		return false;
	}

	public virtual IList<T> ParseValues(IList<string> values)
	{
		return ParseValues(values, strict: false);
	}

	public virtual IList<T> ParseStrictValues(IList<string> values)
	{
		return ParseValues(values, strict: true);
	}

	protected virtual IList<T> ParseValues(IList<string> values, bool strict)
	{
		List<T> list = new List<T>();
		if (values == null)
		{
			return list;
		}
		foreach (string value in values)
		{
			int index = 0;
			while (!string.IsNullOrEmpty(value) && index < value.Length)
			{
				if (TryParseValue(value, ref index, out var parsedValue))
				{
					if (parsedValue != null)
					{
						list.Add(parsedValue);
					}
					continue;
				}
				if (strict)
				{
					throw new FormatException(string.Format(CultureInfo.InvariantCulture, "The header contains invalid values at index {0}: '{1}'", index, value));
				}
				index++;
			}
		}
		return list;
	}

	public virtual string ToString(object value)
	{
		return value.ToString();
	}
}
