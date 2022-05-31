using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class MediaTypeHeaderValue
{
	private const string BoundaryString = "boundary";

	private const string CharsetString = "charset";

	private const string MatchesAllString = "*/*";

	private const string QualityString = "q";

	private const string WildcardString = "*";

	private const char ForwardSlashCharacter = '/';

	private const char PeriodCharacter = '.';

	private const char PlusCharacter = '+';

	private static readonly char[] PeriodCharacterArray = new char[1] { '.' };

	private static readonly HttpHeaderParser<MediaTypeHeaderValue> SingleValueParser = new GenericHeaderParser<MediaTypeHeaderValue>(supportsMultipleValues: false, GetMediaTypeLength);

	private static readonly HttpHeaderParser<MediaTypeHeaderValue> MultipleValueParser = new GenericHeaderParser<MediaTypeHeaderValue>(supportsMultipleValues: true, GetMediaTypeLength);

	private ObjectCollection<NameValueHeaderValue> _parameters;

	private StringSegment _mediaType;

	private bool _isReadOnly;

	public StringSegment Charset
	{
		get
		{
			return NameValueHeaderValue.Find(_parameters, "charset")?.Value.Value;
		}
		set
		{
			HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
			NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, "charset");
			if (StringSegment.IsNullOrEmpty(value))
			{
				if (nameValueHeaderValue != null)
				{
					Parameters.Remove(nameValueHeaderValue);
				}
			}
			else if (nameValueHeaderValue != null)
			{
				nameValueHeaderValue.Value = value;
			}
			else
			{
				Parameters.Add(new NameValueHeaderValue("charset", value));
			}
		}
	}

	public Encoding Encoding
	{
		get
		{
			StringSegment charset = Charset;
			if (!StringSegment.IsNullOrEmpty(charset))
			{
				try
				{
					return Encoding.GetEncoding(charset.Value);
				}
				catch (ArgumentException)
				{
				}
			}
			return null;
		}
		set
		{
			HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
			if (value == null)
			{
				Charset = null;
			}
			else
			{
				Charset = value.WebName;
			}
		}
	}

	public StringSegment Boundary
	{
		get
		{
			return NameValueHeaderValue.Find(_parameters, "boundary")?.Value ?? default(StringSegment);
		}
		set
		{
			HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
			NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, "boundary");
			if (StringSegment.IsNullOrEmpty(value))
			{
				if (nameValueHeaderValue != null)
				{
					Parameters.Remove(nameValueHeaderValue);
				}
			}
			else if (nameValueHeaderValue != null)
			{
				nameValueHeaderValue.Value = value;
			}
			else
			{
				Parameters.Add(new NameValueHeaderValue("boundary", value));
			}
		}
	}

	public IList<NameValueHeaderValue> Parameters
	{
		get
		{
			if (_parameters == null)
			{
				if (IsReadOnly)
				{
					_parameters = ObjectCollection<NameValueHeaderValue>.EmptyReadOnlyCollection;
				}
				else
				{
					_parameters = new ObjectCollection<NameValueHeaderValue>();
				}
			}
			return _parameters;
		}
	}

	public double? Quality
	{
		get
		{
			return HeaderUtilities.GetQuality(_parameters);
		}
		set
		{
			HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
			HeaderUtilities.SetQuality(Parameters, value);
		}
	}

	public StringSegment MediaType
	{
		get
		{
			return _mediaType;
		}
		set
		{
			HeaderUtilities.ThrowIfReadOnly(IsReadOnly);
			CheckMediaTypeFormat(value, "value");
			_mediaType = value;
		}
	}

	public StringSegment Type => _mediaType.Subsegment(0, _mediaType.IndexOf('/'));

	public StringSegment SubType => _mediaType.Subsegment(_mediaType.IndexOf('/') + 1);

	public StringSegment SubTypeWithoutSuffix
	{
		get
		{
			StringSegment subType = SubType;
			int num = subType.LastIndexOf('+');
			if (num == -1)
			{
				return subType;
			}
			return subType.Subsegment(0, num);
		}
	}

	public StringSegment Suffix
	{
		get
		{
			StringSegment subType = SubType;
			int num = subType.LastIndexOf('+');
			if (num == -1)
			{
				return default(StringSegment);
			}
			return subType.Subsegment(num + 1);
		}
	}

	public IEnumerable<StringSegment> Facets => SubTypeWithoutSuffix.Split(PeriodCharacterArray);

	public bool MatchesAllTypes => MediaType.Equals("*/*", StringComparison.Ordinal);

	public bool MatchesAllSubTypes => SubType.Equals("*", StringComparison.Ordinal);

	public bool MatchesAllSubTypesWithoutSuffix => SubTypeWithoutSuffix.Equals("*", StringComparison.OrdinalIgnoreCase);

	public bool IsReadOnly => _isReadOnly;

	private MediaTypeHeaderValue()
	{
	}

	public MediaTypeHeaderValue(StringSegment mediaType)
	{
		CheckMediaTypeFormat(mediaType, "mediaType");
		_mediaType = mediaType;
	}

	public MediaTypeHeaderValue(StringSegment mediaType, double quality)
		: this(mediaType)
	{
		Quality = quality;
	}

	public bool IsSubsetOf(MediaTypeHeaderValue otherMediaType)
	{
		if (otherMediaType == null)
		{
			return false;
		}
		if (MatchesType(otherMediaType) && MatchesSubtype(otherMediaType))
		{
			return MatchesParameters(otherMediaType);
		}
		return false;
	}

	public MediaTypeHeaderValue Copy()
	{
		MediaTypeHeaderValue mediaTypeHeaderValue = new MediaTypeHeaderValue();
		mediaTypeHeaderValue._mediaType = _mediaType;
		if (_parameters != null)
		{
			mediaTypeHeaderValue._parameters = new ObjectCollection<NameValueHeaderValue>(_parameters.Select((NameValueHeaderValue item) => item.Copy()));
		}
		return mediaTypeHeaderValue;
	}

	public MediaTypeHeaderValue CopyAsReadOnly()
	{
		if (IsReadOnly)
		{
			return this;
		}
		MediaTypeHeaderValue mediaTypeHeaderValue = new MediaTypeHeaderValue();
		mediaTypeHeaderValue._mediaType = _mediaType;
		if (_parameters != null)
		{
			mediaTypeHeaderValue._parameters = new ObjectCollection<NameValueHeaderValue>(_parameters.Select((NameValueHeaderValue item) => item.CopyAsReadOnly()), isReadOnly: true);
		}
		mediaTypeHeaderValue._isReadOnly = true;
		return mediaTypeHeaderValue;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((object)_mediaType);
		NameValueHeaderValue.ToString(_parameters, ';', leadingSeparator: true, stringBuilder);
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MediaTypeHeaderValue mediaTypeHeaderValue))
		{
			return false;
		}
		if (_mediaType.Equals(mediaTypeHeaderValue._mediaType, StringComparison.OrdinalIgnoreCase))
		{
			return HeaderUtilities.AreEqualCollections(_parameters, mediaTypeHeaderValue._parameters);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_mediaType) ^ NameValueHeaderValue.GetHashCode(_parameters);
	}

	public static MediaTypeHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return SingleValueParser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out MediaTypeHeaderValue parsedValue)
	{
		int index = 0;
		return SingleValueParser.TryParseValue(input, ref index, out parsedValue);
	}

	public static IList<MediaTypeHeaderValue> ParseList(IList<string> inputs)
	{
		return MultipleValueParser.ParseValues(inputs);
	}

	public static IList<MediaTypeHeaderValue> ParseStrictList(IList<string> inputs)
	{
		return MultipleValueParser.ParseStrictValues(inputs);
	}

	public static bool TryParseList(IList<string> inputs, out IList<MediaTypeHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseValues(inputs, out parsedValues);
	}

	public static bool TryParseStrictList(IList<string> inputs, out IList<MediaTypeHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseStrictValues(inputs, out parsedValues);
	}

	private static int GetMediaTypeLength(StringSegment input, int startIndex, out MediaTypeHeaderValue parsedValue)
	{
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(input) || startIndex >= input.Length)
		{
			return 0;
		}
		StringSegment mediaType;
		int mediaTypeExpressionLength = GetMediaTypeExpressionLength(input, startIndex, out mediaType);
		if (mediaTypeExpressionLength == 0)
		{
			return 0;
		}
		int num = startIndex + mediaTypeExpressionLength;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		MediaTypeHeaderValue mediaTypeHeaderValue = null;
		if (num < input.Length && input[num] == ';')
		{
			mediaTypeHeaderValue = new MediaTypeHeaderValue();
			mediaTypeHeaderValue._mediaType = mediaType;
			num++;
			int nameValueListLength = NameValueHeaderValue.GetNameValueListLength(input, num, ';', mediaTypeHeaderValue.Parameters);
			parsedValue = mediaTypeHeaderValue;
			return num + nameValueListLength - startIndex;
		}
		mediaTypeHeaderValue = new MediaTypeHeaderValue();
		mediaTypeHeaderValue._mediaType = mediaType;
		parsedValue = mediaTypeHeaderValue;
		return num - startIndex;
	}

	private static int GetMediaTypeExpressionLength(StringSegment input, int startIndex, out StringSegment mediaType)
	{
		mediaType = null;
		int tokenLength = HttpRuleParser.GetTokenLength(input, startIndex);
		if (tokenLength == 0)
		{
			return 0;
		}
		int num = startIndex + tokenLength;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		if (num >= input.Length || input[num] != '/')
		{
			return 0;
		}
		num++;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		int tokenLength2 = HttpRuleParser.GetTokenLength(input, num);
		if (tokenLength2 == 0)
		{
			return 0;
		}
		int num2 = num + tokenLength2 - startIndex;
		if (tokenLength + tokenLength2 + 1 == num2)
		{
			mediaType = input.Subsegment(startIndex, num2);
		}
		else
		{
			mediaType = input.Substring(startIndex, tokenLength) + "/" + input.Substring(num, tokenLength2);
		}
		return num2;
	}

	private static void CheckMediaTypeFormat(StringSegment mediaType, string parameterName)
	{
		if (StringSegment.IsNullOrEmpty(mediaType))
		{
			throw new ArgumentException("An empty string is not allowed.", parameterName);
		}
		if (GetMediaTypeExpressionLength(mediaType, 0, out var mediaType2) == 0 || mediaType2.Length != mediaType.Length)
		{
			throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Invalid media type '{0}'.", mediaType));
		}
	}

	private bool MatchesType(MediaTypeHeaderValue set)
	{
		if (!set.MatchesAllTypes)
		{
			return set.Type.Equals(Type, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private bool MatchesSubtype(MediaTypeHeaderValue set)
	{
		if (set.MatchesAllSubTypes)
		{
			return true;
		}
		if (set.Suffix.HasValue)
		{
			if (Suffix.HasValue)
			{
				if (MatchesSubtypeWithoutSuffix(set))
				{
					return MatchesSubtypeSuffix(set);
				}
				return false;
			}
			return false;
		}
		return MatchesEitherSubtypeOrSuffix(set);
	}

	private bool MatchesSubtypeWithoutSuffix(MediaTypeHeaderValue set)
	{
		if (!set.MatchesAllSubTypesWithoutSuffix)
		{
			return set.SubTypeWithoutSuffix.Equals(SubTypeWithoutSuffix, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private bool MatchesEitherSubtypeOrSuffix(MediaTypeHeaderValue set)
	{
		if (!set.SubType.Equals(SubType, StringComparison.OrdinalIgnoreCase))
		{
			return set.SubType.Equals(Suffix, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private bool MatchesParameters(MediaTypeHeaderValue set)
	{
		if (set._parameters != null && set._parameters.Count != 0)
		{
			foreach (NameValueHeaderValue parameter in set._parameters)
			{
				if (!parameter.Name.Equals("*", StringComparison.OrdinalIgnoreCase))
				{
					if (parameter.Name.Equals("q", StringComparison.OrdinalIgnoreCase))
					{
						break;
					}
					NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, parameter.Name);
					if (nameValueHeaderValue == null)
					{
						return false;
					}
					if (!StringSegment.Equals(parameter.Value, nameValueHeaderValue.Value, StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool MatchesSubtypeSuffix(MediaTypeHeaderValue set)
	{
		return set.Suffix.Equals(Suffix, StringComparison.OrdinalIgnoreCase);
	}
}
