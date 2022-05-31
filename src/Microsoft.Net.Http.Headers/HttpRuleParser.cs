using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

internal static class HttpRuleParser
{
	private static readonly bool[] TokenChars = CreateTokenChars();

	private const int MaxNestedCount = 5;

	private static readonly string[] DateFormats = new string[16]
	{
		"ddd, d MMM yyyy H:m:s 'GMT'", "ddd, d MMM yyyy H:m:s", "d MMM yyyy H:m:s 'GMT'", "d MMM yyyy H:m:s", "ddd, d MMM yy H:m:s 'GMT'", "ddd, d MMM yy H:m:s", "d MMM yy H:m:s 'GMT'", "d MMM yy H:m:s", "dddd, d'-'MMM'-'yy H:m:s 'GMT'", "dddd, d'-'MMM'-'yy H:m:s",
		"ddd, d'-'MMM'-'yyyy H:m:s 'GMT'", "ddd MMM d H:m:s yyyy", "ddd, d MMM yyyy H:m:s zzz", "ddd, d MMM yyyy H:m:s", "d MMM yyyy H:m:s zzz", "d MMM yyyy H:m:s"
	};

	internal const char CR = '\r';

	internal const char LF = '\n';

	internal const char SP = ' ';

	internal const char Tab = '\t';

	internal const int MaxInt64Digits = 19;

	internal const int MaxInt32Digits = 10;

	internal static readonly Encoding DefaultHttpEncoding = Encoding.GetEncoding("iso-8859-1");

	private static bool[] CreateTokenChars()
	{
		bool[] array = new bool[128];
		for (int i = 33; i < 127; i++)
		{
			array[i] = true;
		}
		array[40] = false;
		array[41] = false;
		array[60] = false;
		array[62] = false;
		array[64] = false;
		array[44] = false;
		array[59] = false;
		array[58] = false;
		array[92] = false;
		array[34] = false;
		array[47] = false;
		array[91] = false;
		array[93] = false;
		array[63] = false;
		array[61] = false;
		array[123] = false;
		array[125] = false;
		return array;
	}

	internal static bool IsTokenChar(char character)
	{
		if (character > '\u007f')
		{
			return false;
		}
		return TokenChars[(uint)character];
	}

	internal static int GetTokenLength(StringSegment input, int startIndex)
	{
		if (startIndex >= input.Length)
		{
			return 0;
		}
		for (int i = startIndex; i < input.Length; i++)
		{
			if (!IsTokenChar(input[i]))
			{
				return i - startIndex;
			}
		}
		return input.Length - startIndex;
	}

	internal static int GetWhitespaceLength(StringSegment input, int startIndex)
	{
		if (startIndex >= input.Length)
		{
			return 0;
		}
		int num = startIndex;
		while (num < input.Length)
		{
			switch (input[num])
			{
			case '\t':
			case ' ':
				num++;
				continue;
			case '\r':
				if (num + 2 < input.Length && input[num + 1] == '\n')
				{
					char c = input[num + 2];
					if (c == ' ' || c == '\t')
					{
						num += 3;
						continue;
					}
				}
				break;
			}
			return num - startIndex;
		}
		return input.Length - startIndex;
	}

	internal static int GetNumberLength(StringSegment input, int startIndex, bool allowDecimal)
	{
		int num = startIndex;
		bool flag = !allowDecimal;
		if (input[num] == '.')
		{
			return 0;
		}
		while (num < input.Length)
		{
			char c = input[num];
			if (c >= '0' && c <= '9')
			{
				num++;
				continue;
			}
			if (flag || c != '.')
			{
				break;
			}
			flag = true;
			num++;
		}
		return num - startIndex;
	}

	internal static HttpParseResult GetQuotedStringLength(StringSegment input, int startIndex, out int length)
	{
		int nestedCount = 0;
		return GetExpressionLength(input, startIndex, '"', '"', supportsNesting: false, ref nestedCount, out length);
	}

	internal static HttpParseResult GetQuotedPairLength(StringSegment input, int startIndex, out int length)
	{
		length = 0;
		if (input[startIndex] != '\\')
		{
			return HttpParseResult.NotParsed;
		}
		if (startIndex + 2 > input.Length || input[startIndex + 1] > '\u007f')
		{
			return HttpParseResult.InvalidFormat;
		}
		length = 2;
		return HttpParseResult.Parsed;
	}

	internal static bool TryStringToDate(StringSegment input, out DateTimeOffset result)
	{
		return DateTimeOffset.TryParseExact(input.ToString(), DateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out result);
	}

	private static HttpParseResult GetExpressionLength(StringSegment input, int startIndex, char openChar, char closeChar, bool supportsNesting, ref int nestedCount, out int length)
	{
		length = 0;
		if (input[startIndex] != openChar)
		{
			return HttpParseResult.NotParsed;
		}
		int num = startIndex + 1;
		while (num < input.Length)
		{
			int length2 = 0;
			if (num + 2 < input.Length && GetQuotedPairLength(input, num, out length2) == HttpParseResult.Parsed)
			{
				num += length2;
				continue;
			}
			if (supportsNesting && input[num] == openChar)
			{
				nestedCount++;
				try
				{
					if (nestedCount > 5)
					{
						return HttpParseResult.InvalidFormat;
					}
					int length3 = 0;
					switch (GetExpressionLength(input, num, openChar, closeChar, supportsNesting, ref nestedCount, out length3))
					{
					case HttpParseResult.Parsed:
						num += length3;
						break;
					case HttpParseResult.InvalidFormat:
						return HttpParseResult.InvalidFormat;
					case HttpParseResult.NotParsed:
						break;
					}
				}
				finally
				{
					nestedCount--;
				}
			}
			if (input[num] == closeChar)
			{
				length = num - startIndex + 1;
				return HttpParseResult.Parsed;
			}
			num++;
		}
		return HttpParseResult.InvalidFormat;
	}
}
