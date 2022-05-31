using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class SetCookieHeaderValue
{
	private const string ExpiresToken = "expires";

	private const string MaxAgeToken = "max-age";

	private const string DomainToken = "domain";

	private const string PathToken = "path";

	private const string SecureToken = "secure";

	private const string SameSiteToken = "samesite";

	private static readonly string SameSiteNoneToken;

	private static readonly string SameSiteLaxToken;

	private static readonly string SameSiteStrictToken;

	internal static bool SuppressSameSiteNone;

	private const string HttpOnlyToken = "httponly";

	private const string SeparatorToken = "; ";

	private const string EqualsToken = "=";

	private const string DefaultPath = "/";

	private static readonly HttpHeaderParser<SetCookieHeaderValue> SingleValueParser;

	private static readonly HttpHeaderParser<SetCookieHeaderValue> MultipleValueParser;

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
			CookieHeaderValue.CheckNameFormat(value, "value");
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
			CookieHeaderValue.CheckValueFormat(value, "value");
			_value = value;
		}
	}

	public DateTimeOffset? Expires { get; set; }

	public TimeSpan? MaxAge { get; set; }

	public StringSegment Domain { get; set; }

	public StringSegment Path { get; set; }

	public bool Secure { get; set; }

	public SameSiteMode SameSite { get; set; } = (!SuppressSameSiteNone) ? ((SameSiteMode)(-1)) : SameSiteMode.None;


	public bool HttpOnly { get; set; }

	static SetCookieHeaderValue()
	{
		SameSiteNoneToken = SameSiteMode.None.ToString().ToLower();
		SameSiteLaxToken = SameSiteMode.Lax.ToString().ToLower();
		SameSiteStrictToken = SameSiteMode.Strict.ToString().ToLower();
		SingleValueParser = new GenericHeaderParser<SetCookieHeaderValue>(supportsMultipleValues: false, GetSetCookieLength);
		MultipleValueParser = new GenericHeaderParser<SetCookieHeaderValue>(supportsMultipleValues: true, GetSetCookieLength);
		if (AppContext.TryGetSwitch("Microsoft.AspNetCore.SuppressSameSiteNone", out var isEnabled))
		{
			SuppressSameSiteNone = isEnabled;
		}
	}

	private SetCookieHeaderValue()
	{
	}

	public SetCookieHeaderValue(StringSegment name)
		: this(name, StringSegment.Empty)
	{
	}

	public SetCookieHeaderValue(StringSegment name, StringSegment value)
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
		int num = _name.Length + "=".Length + _value.Length;
		string text = null;
		string text2 = null;
		string text3 = null;
		if (Expires.HasValue)
		{
			text = HeaderUtilities.FormatDate(Expires.Value);
			num += "; ".Length + "expires".Length + "=".Length + text.Length;
		}
		if (MaxAge.HasValue)
		{
			text2 = HeaderUtilities.FormatNonNegativeInt64((long)MaxAge.Value.TotalSeconds);
			num += "; ".Length + "max-age".Length + "=".Length + text2.Length;
		}
		if (Domain != null)
		{
			num += "; ".Length + "domain".Length + "=".Length + Domain.Length;
		}
		if (Path != null)
		{
			num += "; ".Length + "path".Length + "=".Length + Path.Length;
		}
		if (Secure)
		{
			num += "; ".Length + "secure".Length;
		}
		if (SameSite == SameSiteMode.None && !SuppressSameSiteNone)
		{
			text3 = SameSiteNoneToken;
			num += "; ".Length + "samesite".Length + "=".Length + text3.Length;
		}
		else if (SameSite == SameSiteMode.Lax)
		{
			text3 = SameSiteLaxToken;
			num += "; ".Length + "samesite".Length + "=".Length + text3.Length;
		}
		else if (SameSite == SameSiteMode.Strict)
		{
			text3 = SameSiteStrictToken;
			num += "; ".Length + "samesite".Length + "=".Length + text3.Length;
		}
		if (HttpOnly)
		{
			num += "; ".Length + "httponly".Length;
		}
		InplaceStringBuilder builder = new InplaceStringBuilder(num);
		((InplaceStringBuilder)(builder)).Append(_name);
		((InplaceStringBuilder)(builder)).Append("=");
		((InplaceStringBuilder)(builder)).Append(_value);
		if (text != null)
		{
			AppendSegment(ref builder, "expires", text);
		}
		if (text2 != null)
		{
			AppendSegment(ref builder, "max-age", text2);
		}
		if (Domain != null)
		{
			AppendSegment(ref builder, "domain", Domain);
		}
		if (Path != null)
		{
			AppendSegment(ref builder, "path", Path);
		}
		if (Secure)
		{
			AppendSegment(ref builder, "secure", null);
		}
		if (text3 != null)
		{
			AppendSegment(ref builder, "samesite", text3);
		}
		if (HttpOnly)
		{
			AppendSegment(ref builder, "httponly", null);
		}
		return ((object)builder).ToString();
	}

	private static void AppendSegment(ref InplaceStringBuilder builder, StringSegment name, StringSegment value)
	{
		((InplaceStringBuilder)(builder)).Append("; ");
		((InplaceStringBuilder)(builder)).Append(name);
		if (value != null)
		{
			((InplaceStringBuilder)(builder)).Append("=");
			((InplaceStringBuilder)(builder)).Append(value);
		}
	}

	public void AppendToStringBuilder(StringBuilder builder)
	{
		builder.Append((object)_name);
		builder.Append("=");
		builder.Append((object)_value);
		if (Expires.HasValue)
		{
			AppendSegment(builder, "expires", HeaderUtilities.FormatDate(Expires.Value));
		}
		if (MaxAge.HasValue)
		{
			AppendSegment(builder, "max-age", HeaderUtilities.FormatNonNegativeInt64((long)MaxAge.Value.TotalSeconds));
		}
		if (Domain != null)
		{
			AppendSegment(builder, "domain", Domain);
		}
		if (Path != null)
		{
			AppendSegment(builder, "path", Path);
		}
		if (Secure)
		{
			AppendSegment(builder, "secure", null);
		}
		if (SameSite == SameSiteMode.None && !SuppressSameSiteNone)
		{
			AppendSegment(builder, "samesite", SameSiteNoneToken);
		}
		else if (SameSite == SameSiteMode.Lax)
		{
			AppendSegment(builder, "samesite", SameSiteLaxToken);
		}
		else if (SameSite == SameSiteMode.Strict)
		{
			AppendSegment(builder, "samesite", SameSiteStrictToken);
		}
		if (HttpOnly)
		{
			AppendSegment(builder, "httponly", null);
		}
	}

	private static void AppendSegment(StringBuilder builder, StringSegment name, StringSegment value)
	{
		builder.Append("; ");
		builder.Append((object)name);
		if (value != null)
		{
			builder.Append("=");
			builder.Append((object)value);
		}
	}

	public static SetCookieHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return SingleValueParser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out SetCookieHeaderValue parsedValue)
	{
		int index = 0;
		return SingleValueParser.TryParseValue(input, ref index, out parsedValue);
	}

	public static IList<SetCookieHeaderValue> ParseList(IList<string> inputs)
	{
		return MultipleValueParser.ParseValues(inputs);
	}

	public static IList<SetCookieHeaderValue> ParseStrictList(IList<string> inputs)
	{
		return MultipleValueParser.ParseStrictValues(inputs);
	}

	public static bool TryParseList(IList<string> inputs, out IList<SetCookieHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseValues(inputs, out parsedValues);
	}

	public static bool TryParseStrictList(IList<string> inputs, out IList<SetCookieHeaderValue> parsedValues)
	{
		return MultipleValueParser.TryParseStrictValues(inputs, out parsedValues);
	}

	private static int GetSetCookieLength(StringSegment input, int startIndex, out SetCookieHeaderValue parsedValue)
	{
		int num = startIndex;
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(input) || num >= input.Length)
		{
			return 0;
		}
		SetCookieHeaderValue setCookieHeaderValue = new SetCookieHeaderValue();
		int tokenLength = HttpRuleParser.GetTokenLength(input, num);
		if (tokenLength == 0)
		{
			return 0;
		}
		setCookieHeaderValue._name = input.Subsegment(num, tokenLength);
		num += tokenLength;
		if (!ReadEqualsSign(input, ref num))
		{
			return 0;
		}
		setCookieHeaderValue._value = CookieHeaderValue.GetCookieValue(input, ref num);
		while (num < input.Length && input[num] != ',')
		{
			if (input[num] != ';')
			{
				return 0;
			}
			num++;
			num += HttpRuleParser.GetWhitespaceLength(input, num);
			tokenLength = HttpRuleParser.GetTokenLength(input, num);
			if (tokenLength == 0)
			{
				break;
			}
			StringSegment a = input.Subsegment(num, tokenLength);
			num += tokenLength;
			if (StringSegment.Equals(a, "expires", StringComparison.OrdinalIgnoreCase))
			{
				if (!ReadEqualsSign(input, ref num))
				{
					return 0;
				}
				if (!HttpRuleParser.TryStringToDate(ReadToSemicolonOrEnd(input, ref num), out var result))
				{
					return 0;
				}
				setCookieHeaderValue.Expires = result;
			}
			else if (StringSegment.Equals(a, "max-age", StringComparison.OrdinalIgnoreCase))
			{
				if (!ReadEqualsSign(input, ref num))
				{
					return 0;
				}
				tokenLength = HttpRuleParser.GetNumberLength(input, num, allowDecimal: false);
				if (tokenLength == 0)
				{
					return 0;
				}
				if (!HeaderUtilities.TryParseNonNegativeInt64(input.Subsegment(num, tokenLength), out var result2))
				{
					return 0;
				}
				setCookieHeaderValue.MaxAge = TimeSpan.FromSeconds(result2);
				num += tokenLength;
			}
			else if (StringSegment.Equals(a, "domain", StringComparison.OrdinalIgnoreCase))
			{
				if (!ReadEqualsSign(input, ref num))
				{
					return 0;
				}
				setCookieHeaderValue.Domain = ReadToSemicolonOrEnd(input, ref num);
			}
			else if (StringSegment.Equals(a, "path", StringComparison.OrdinalIgnoreCase))
			{
				if (!ReadEqualsSign(input, ref num))
				{
					return 0;
				}
				setCookieHeaderValue.Path = ReadToSemicolonOrEnd(input, ref num);
			}
			else if (StringSegment.Equals(a, "secure", StringComparison.OrdinalIgnoreCase))
			{
				setCookieHeaderValue.Secure = true;
			}
			else if (StringSegment.Equals(a, "samesite", StringComparison.OrdinalIgnoreCase))
			{
				if (!ReadEqualsSign(input, ref num))
				{
					setCookieHeaderValue.SameSite = (SuppressSameSiteNone ? SameSiteMode.Strict : ((SameSiteMode)(-1)));
					continue;
				}
				StringSegment a2 = ReadToSemicolonOrEnd(input, ref num);
				if (StringSegment.Equals(a2, SameSiteStrictToken, StringComparison.OrdinalIgnoreCase))
				{
					setCookieHeaderValue.SameSite = SameSiteMode.Strict;
				}
				else if (StringSegment.Equals(a2, SameSiteLaxToken, StringComparison.OrdinalIgnoreCase))
				{
					setCookieHeaderValue.SameSite = SameSiteMode.Lax;
				}
				else if (!SuppressSameSiteNone && StringSegment.Equals(a2, SameSiteNoneToken, StringComparison.OrdinalIgnoreCase))
				{
					setCookieHeaderValue.SameSite = SameSiteMode.None;
				}
				else
				{
					setCookieHeaderValue.SameSite = (SuppressSameSiteNone ? SameSiteMode.Strict : ((SameSiteMode)(-1)));
				}
			}
			else if (StringSegment.Equals(a, "httponly", StringComparison.OrdinalIgnoreCase))
			{
				setCookieHeaderValue.HttpOnly = true;
			}
		}
		parsedValue = setCookieHeaderValue;
		return num - startIndex;
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

	private static StringSegment ReadToSemicolonOrEnd(StringSegment input, ref int offset)
	{
		int num = input.IndexOf(';', offset);
		if (num < 0)
		{
			num = input.Length;
		}
		int num2 = num - offset;
		StringSegment result = input.Subsegment(offset, num2);
		offset += num2;
		return result;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SetCookieHeaderValue setCookieHeaderValue))
		{
			return false;
		}
		if (StringSegment.Equals(_name, setCookieHeaderValue._name, StringComparison.OrdinalIgnoreCase) && StringSegment.Equals(_value, setCookieHeaderValue._value, StringComparison.OrdinalIgnoreCase) && Expires.Equals(setCookieHeaderValue.Expires) && MaxAge.Equals(setCookieHeaderValue.MaxAge) && StringSegment.Equals(Domain, setCookieHeaderValue.Domain, StringComparison.OrdinalIgnoreCase) && StringSegment.Equals(Path, setCookieHeaderValue.Path, StringComparison.OrdinalIgnoreCase) && Secure == setCookieHeaderValue.Secure && SameSite == setCookieHeaderValue.SameSite)
		{
			return HttpOnly == setCookieHeaderValue.HttpOnly;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_name) ^ StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_value) ^ (Expires.HasValue ? Expires.GetHashCode() : 0) ^ (MaxAge.HasValue ? MaxAge.GetHashCode() : 0) ^ ((Domain != null) ? StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(Domain) : 0) ^ ((Path != null) ? StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(Path) : 0) ^ Secure.GetHashCode() ^ SameSite.GetHashCode() ^ HttpOnly.GetHashCode();
	}
}
