using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class CacheControlHeaderValue
{
	public static readonly string PublicString = "public";

	public static readonly string PrivateString = "private";

	public static readonly string MaxAgeString = "max-age";

	public static readonly string SharedMaxAgeString = "s-maxage";

	public static readonly string NoCacheString = "no-cache";

	public static readonly string NoStoreString = "no-store";

	public static readonly string MaxStaleString = "max-stale";

	public static readonly string MinFreshString = "min-fresh";

	public static readonly string NoTransformString = "no-transform";

	public static readonly string OnlyIfCachedString = "only-if-cached";

	public static readonly string MustRevalidateString = "must-revalidate";

	public static readonly string ProxyRevalidateString = "proxy-revalidate";

	private static readonly HttpHeaderParser<CacheControlHeaderValue> Parser = new GenericHeaderParser<CacheControlHeaderValue>(supportsMultipleValues: true, GetCacheControlLength);

	private static readonly Action<StringSegment> CheckIsValidTokenAction = CheckIsValidToken;

	private bool _noCache;

	private ICollection<StringSegment> _noCacheHeaders;

	private bool _noStore;

	private TimeSpan? _maxAge;

	private TimeSpan? _sharedMaxAge;

	private bool _maxStale;

	private TimeSpan? _maxStaleLimit;

	private TimeSpan? _minFresh;

	private bool _noTransform;

	private bool _onlyIfCached;

	private bool _public;

	private bool _private;

	private ICollection<StringSegment> _privateHeaders;

	private bool _mustRevalidate;

	private bool _proxyRevalidate;

	private IList<NameValueHeaderValue> _extensions;

	public bool NoCache
	{
		get
		{
			return _noCache;
		}
		set
		{
			_noCache = value;
		}
	}

	public ICollection<StringSegment> NoCacheHeaders
	{
		get
		{
			if (_noCacheHeaders == null)
			{
				_noCacheHeaders = new ObjectCollection<StringSegment>(CheckIsValidTokenAction);
			}
			return _noCacheHeaders;
		}
	}

	public bool NoStore
	{
		get
		{
			return _noStore;
		}
		set
		{
			_noStore = value;
		}
	}

	public TimeSpan? MaxAge
	{
		get
		{
			return _maxAge;
		}
		set
		{
			_maxAge = value;
		}
	}

	public TimeSpan? SharedMaxAge
	{
		get
		{
			return _sharedMaxAge;
		}
		set
		{
			_sharedMaxAge = value;
		}
	}

	public bool MaxStale
	{
		get
		{
			return _maxStale;
		}
		set
		{
			_maxStale = value;
		}
	}

	public TimeSpan? MaxStaleLimit
	{
		get
		{
			return _maxStaleLimit;
		}
		set
		{
			_maxStaleLimit = value;
		}
	}

	public TimeSpan? MinFresh
	{
		get
		{
			return _minFresh;
		}
		set
		{
			_minFresh = value;
		}
	}

	public bool NoTransform
	{
		get
		{
			return _noTransform;
		}
		set
		{
			_noTransform = value;
		}
	}

	public bool OnlyIfCached
	{
		get
		{
			return _onlyIfCached;
		}
		set
		{
			_onlyIfCached = value;
		}
	}

	public bool Public
	{
		get
		{
			return _public;
		}
		set
		{
			_public = value;
		}
	}

	public bool Private
	{
		get
		{
			return _private;
		}
		set
		{
			_private = value;
		}
	}

	public ICollection<StringSegment> PrivateHeaders
	{
		get
		{
			if (_privateHeaders == null)
			{
				_privateHeaders = new ObjectCollection<StringSegment>(CheckIsValidTokenAction);
			}
			return _privateHeaders;
		}
	}

	public bool MustRevalidate
	{
		get
		{
			return _mustRevalidate;
		}
		set
		{
			_mustRevalidate = value;
		}
	}

	public bool ProxyRevalidate
	{
		get
		{
			return _proxyRevalidate;
		}
		set
		{
			_proxyRevalidate = value;
		}
	}

	public IList<NameValueHeaderValue> Extensions
	{
		get
		{
			if (_extensions == null)
			{
				_extensions = new ObjectCollection<NameValueHeaderValue>();
			}
			return _extensions;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendValueIfRequired(stringBuilder, _noStore, NoStoreString);
		AppendValueIfRequired(stringBuilder, _noTransform, NoTransformString);
		AppendValueIfRequired(stringBuilder, _onlyIfCached, OnlyIfCachedString);
		AppendValueIfRequired(stringBuilder, _public, PublicString);
		AppendValueIfRequired(stringBuilder, _mustRevalidate, MustRevalidateString);
		AppendValueIfRequired(stringBuilder, _proxyRevalidate, ProxyRevalidateString);
		if (_noCache)
		{
			AppendValueWithSeparatorIfRequired(stringBuilder, NoCacheString);
			if (_noCacheHeaders != null && _noCacheHeaders.Count > 0)
			{
				stringBuilder.Append("=\"");
				AppendValues(stringBuilder, _noCacheHeaders);
				stringBuilder.Append('"');
			}
		}
		if (_maxAge.HasValue)
		{
			AppendValueWithSeparatorIfRequired(stringBuilder, MaxAgeString);
			stringBuilder.Append('=');
			stringBuilder.Append(((int)_maxAge.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
		}
		if (_sharedMaxAge.HasValue)
		{
			AppendValueWithSeparatorIfRequired(stringBuilder, SharedMaxAgeString);
			stringBuilder.Append('=');
			stringBuilder.Append(((int)_sharedMaxAge.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
		}
		if (_maxStale)
		{
			AppendValueWithSeparatorIfRequired(stringBuilder, MaxStaleString);
			if (_maxStaleLimit.HasValue)
			{
				stringBuilder.Append('=');
				stringBuilder.Append(((int)_maxStaleLimit.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
			}
		}
		if (_minFresh.HasValue)
		{
			AppendValueWithSeparatorIfRequired(stringBuilder, MinFreshString);
			stringBuilder.Append('=');
			stringBuilder.Append(((int)_minFresh.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
		}
		if (_private)
		{
			AppendValueWithSeparatorIfRequired(stringBuilder, PrivateString);
			if (_privateHeaders != null && _privateHeaders.Count > 0)
			{
				stringBuilder.Append("=\"");
				AppendValues(stringBuilder, _privateHeaders);
				stringBuilder.Append('"');
			}
		}
		NameValueHeaderValue.ToString(_extensions, ',', leadingSeparator: false, stringBuilder);
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CacheControlHeaderValue cacheControlHeaderValue))
		{
			return false;
		}
		if (_noCache == cacheControlHeaderValue._noCache && _noStore == cacheControlHeaderValue._noStore && !(_maxAge != cacheControlHeaderValue._maxAge))
		{
			TimeSpan? sharedMaxAge = _sharedMaxAge;
			TimeSpan? sharedMaxAge2 = cacheControlHeaderValue._sharedMaxAge;
			if (sharedMaxAge.HasValue == sharedMaxAge2.HasValue && (!sharedMaxAge.HasValue || !(sharedMaxAge.GetValueOrDefault() != sharedMaxAge2.GetValueOrDefault())) && _maxStale == cacheControlHeaderValue._maxStale && !(_maxStaleLimit != cacheControlHeaderValue._maxStaleLimit))
			{
				sharedMaxAge = _minFresh;
				sharedMaxAge2 = cacheControlHeaderValue._minFresh;
				if (sharedMaxAge.HasValue == sharedMaxAge2.HasValue && (!sharedMaxAge.HasValue || !(sharedMaxAge.GetValueOrDefault() != sharedMaxAge2.GetValueOrDefault())) && _noTransform == cacheControlHeaderValue._noTransform && _onlyIfCached == cacheControlHeaderValue._onlyIfCached && _public == cacheControlHeaderValue._public && _private == cacheControlHeaderValue._private && _mustRevalidate == cacheControlHeaderValue._mustRevalidate && _proxyRevalidate == cacheControlHeaderValue._proxyRevalidate)
				{
					if (!HeaderUtilities.AreEqualCollections(_noCacheHeaders, cacheControlHeaderValue._noCacheHeaders, StringSegmentComparer.OrdinalIgnoreCase))
					{
						return false;
					}
					if (!HeaderUtilities.AreEqualCollections(_privateHeaders, cacheControlHeaderValue._privateHeaders, StringSegmentComparer.OrdinalIgnoreCase))
					{
						return false;
					}
					if (!HeaderUtilities.AreEqualCollections(_extensions, cacheControlHeaderValue._extensions))
					{
						return false;
					}
					return true;
				}
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = _noCache.GetHashCode() ^ (_noStore.GetHashCode() << 1) ^ (_maxStale.GetHashCode() << 2) ^ (_noTransform.GetHashCode() << 3) ^ (_onlyIfCached.GetHashCode() << 4) ^ (_public.GetHashCode() << 5) ^ (_private.GetHashCode() << 6) ^ (_mustRevalidate.GetHashCode() << 7) ^ (_proxyRevalidate.GetHashCode() << 8);
		num = num ^ (_maxAge.HasValue ? (_maxAge.Value.GetHashCode() ^ 1) : 0) ^ (_sharedMaxAge.HasValue ? (_sharedMaxAge.Value.GetHashCode() ^ 2) : 0) ^ (_maxStaleLimit.HasValue ? (_maxStaleLimit.Value.GetHashCode() ^ 4) : 0) ^ (_minFresh.HasValue ? (_minFresh.Value.GetHashCode() ^ 8) : 0);
		if (_noCacheHeaders != null && _noCacheHeaders.Count > 0)
		{
			foreach (StringSegment noCacheHeader in _noCacheHeaders)
			{
				num ^= StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(noCacheHeader);
			}
		}
		if (_privateHeaders != null && _privateHeaders.Count > 0)
		{
			foreach (StringSegment privateHeader in _privateHeaders)
			{
				num ^= StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(privateHeader);
			}
		}
		if (_extensions != null && _extensions.Count > 0)
		{
			foreach (NameValueHeaderValue extension in _extensions)
			{
				num ^= extension.GetHashCode();
			}
			return num;
		}
		return num;
	}

	public static CacheControlHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return Parser.ParseValue(input, ref index) ?? throw new FormatException("No cache directives found.");
	}

	public static bool TryParse(StringSegment input, out CacheControlHeaderValue parsedValue)
	{
		int index = 0;
		if (Parser.TryParseValue(input, ref index, out parsedValue) && parsedValue != null)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	private static int GetCacheControlLength(StringSegment input, int startIndex, out CacheControlHeaderValue parsedValue)
	{
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(input) || startIndex >= input.Length)
		{
			return 0;
		}
		int index = startIndex;
		NameValueHeaderValue parsedValue2 = null;
		List<NameValueHeaderValue> list = new List<NameValueHeaderValue>();
		while (index < input.Length)
		{
			if (!NameValueHeaderValue.MultipleValueParser.TryParseValue(input, ref index, out parsedValue2))
			{
				return 0;
			}
			list.Add(parsedValue2);
		}
		CacheControlHeaderValue cacheControlHeaderValue = new CacheControlHeaderValue();
		if (!TrySetCacheControlValues(cacheControlHeaderValue, list))
		{
			return 0;
		}
		parsedValue = cacheControlHeaderValue;
		return input.Length - startIndex;
	}

	private static bool TrySetCacheControlValues(CacheControlHeaderValue cc, List<NameValueHeaderValue> nameValueList)
	{
		for (int i = 0; i < nameValueList.Count; i++)
		{
			NameValueHeaderValue nameValueHeaderValue = nameValueList[i];
			StringSegment name = nameValueHeaderValue.Name;
			bool flag = true;
			switch (name.Length)
			{
			case 6:
				if (StringSegment.Equals(PublicString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTokenOnlyValue(nameValueHeaderValue, ref cc._public);
					break;
				}
				goto default;
			case 7:
				if (StringSegment.Equals(MaxAgeString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTimeSpan(nameValueHeaderValue, ref cc._maxAge);
					break;
				}
				if (StringSegment.Equals(PrivateString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetOptionalTokenList(nameValueHeaderValue, ref cc._private, ref cc._privateHeaders);
					break;
				}
				goto default;
			case 8:
				if (StringSegment.Equals(NoCacheString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetOptionalTokenList(nameValueHeaderValue, ref cc._noCache, ref cc._noCacheHeaders);
					break;
				}
				if (StringSegment.Equals(NoStoreString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTokenOnlyValue(nameValueHeaderValue, ref cc._noStore);
					break;
				}
				if (StringSegment.Equals(SharedMaxAgeString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTimeSpan(nameValueHeaderValue, ref cc._sharedMaxAge);
					break;
				}
				goto default;
			case 9:
				if (StringSegment.Equals(MaxStaleString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = nameValueHeaderValue.Value == null || TrySetTimeSpan(nameValueHeaderValue, ref cc._maxStaleLimit);
					if (flag)
					{
						cc._maxStale = true;
					}
					break;
				}
				if (StringSegment.Equals(MinFreshString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTimeSpan(nameValueHeaderValue, ref cc._minFresh);
					break;
				}
				goto default;
			case 12:
				if (StringSegment.Equals(NoTransformString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTokenOnlyValue(nameValueHeaderValue, ref cc._noTransform);
					break;
				}
				goto default;
			case 14:
				if (StringSegment.Equals(OnlyIfCachedString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTokenOnlyValue(nameValueHeaderValue, ref cc._onlyIfCached);
					break;
				}
				goto default;
			case 15:
				if (StringSegment.Equals(MustRevalidateString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTokenOnlyValue(nameValueHeaderValue, ref cc._mustRevalidate);
					break;
				}
				goto default;
			case 16:
				if (StringSegment.Equals(ProxyRevalidateString, name, StringComparison.OrdinalIgnoreCase))
				{
					flag = TrySetTokenOnlyValue(nameValueHeaderValue, ref cc._proxyRevalidate);
					break;
				}
				goto default;
			default:
				cc.Extensions.Add(nameValueHeaderValue);
				break;
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private static bool TrySetTokenOnlyValue(NameValueHeaderValue nameValue, ref bool boolField)
	{
		if (nameValue.Value != null)
		{
			return false;
		}
		boolField = true;
		return true;
	}

	private static bool TrySetOptionalTokenList(NameValueHeaderValue nameValue, ref bool boolField, ref ICollection<StringSegment> destination)
	{
		if (nameValue.Value == null)
		{
			boolField = true;
			return true;
		}
		StringSegment value = nameValue.Value;
		if (value.Length < 3 || value[0] != '"' || value[value.Length - 1] != '"')
		{
			return false;
		}
		int num = 1;
		int num2 = value.Length - 1;
		bool separatorFound = false;
		int num3 = ((destination != null) ? destination.Count : 0);
		while (num < num2)
		{
			num = HeaderUtilities.GetNextNonEmptyOrWhitespaceIndex(value, num, skipEmptyValues: true, out separatorFound);
			if (num == num2)
			{
				break;
			}
			int tokenLength = HttpRuleParser.GetTokenLength(value, num);
			if (tokenLength == 0)
			{
				return false;
			}
			if (destination == null)
			{
				destination = new ObjectCollection<StringSegment>(CheckIsValidTokenAction);
			}
			destination.Add(value.Subsegment(num, tokenLength));
			num += tokenLength;
		}
		if (destination != null && destination.Count > num3)
		{
			boolField = true;
			return true;
		}
		return false;
	}

	private static bool TrySetTimeSpan(NameValueHeaderValue nameValue, ref TimeSpan? timeSpan)
	{
		if (nameValue.Value == null)
		{
			return false;
		}
		if (!HeaderUtilities.TryParseNonNegativeInt32(nameValue.Value, out var result))
		{
			return false;
		}
		timeSpan = new TimeSpan(0, 0, result);
		return true;
	}

	private static void AppendValueIfRequired(StringBuilder sb, bool appendValue, string value)
	{
		if (appendValue)
		{
			AppendValueWithSeparatorIfRequired(sb, value);
		}
	}

	private static void AppendValueWithSeparatorIfRequired(StringBuilder sb, string value)
	{
		if (sb.Length > 0)
		{
			sb.Append(", ");
		}
		sb.Append(value);
	}

	private static void AppendValues(StringBuilder sb, IEnumerable<StringSegment> values)
	{
		bool flag = true;
		foreach (StringSegment value in values)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sb.Append(", ");
			}
			sb.Append((object)value);
		}
	}

	private static void CheckIsValidToken(StringSegment item)
	{
		HeaderUtilities.CheckValidToken(item, "item");
	}
}
