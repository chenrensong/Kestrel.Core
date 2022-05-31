using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public static class HeaderUtilities
{
    private static readonly int _int64MaxStringLength = 19;

    private static readonly int _qualityValueMaxCharCount = 10;

    private const string QualityName = "q";

    internal const string BytesUnit = "bytes";

    internal static void SetQuality(IList<NameValueHeaderValue> parameters, double? value)
    {
        NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(parameters, "q");
        if (value.HasValue)
        {
            if (value < 0.0 || value > 1.0)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            string text = value.Value.ToString("0.0##", NumberFormatInfo.InvariantInfo);
            if (nameValueHeaderValue != null)
            {
                nameValueHeaderValue.Value = text;
            }
            else
            {
                parameters.Add(new NameValueHeaderValue("q", text));
            }
        }
        else if (nameValueHeaderValue != null)
        {
            parameters.Remove(nameValueHeaderValue);
        }
    }

    internal static double? GetQuality(IList<NameValueHeaderValue> parameters)
    {
        NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(parameters, "q");
        if (nameValueHeaderValue != null && TryParseQualityDouble(nameValueHeaderValue.Value, 0, out var quality, out var _))
        {
            return quality;
        }
        return null;
    }

    internal static void CheckValidToken(StringSegment value, string parameterName)
    {
        if (StringSegment.IsNullOrEmpty(value))
        {
            throw new ArgumentException("An empty string is not allowed.", parameterName);
        }
        if (HttpRuleParser.GetTokenLength(value, 0) != value.Length)
        {
            throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Invalid token '{0}.", value));
        }
    }

    internal static bool AreEqualCollections<T>(ICollection<T> x, ICollection<T> y)
    {
        return AreEqualCollections(x, y, null);
    }

    internal static bool AreEqualCollections<T>(ICollection<T> x, ICollection<T> y, IEqualityComparer<T> comparer)
    {
        if (x == null)
        {
            if (y != null)
            {
                return y.Count == 0;
            }
            return true;
        }
        if (y == null)
        {
            return x.Count == 0;
        }
        if (x.Count != y.Count)
        {
            return false;
        }
        if (x.Count == 0)
        {
            return true;
        }
        bool[] array = new bool[x.Count];
        int num = 0;
        foreach (T item in x)
        {
            num = 0;
            bool flag = false;
            foreach (T item2 in y)
            {
                if (!array[num] && ((comparer == null && item.Equals(item2)) || (comparer != null && comparer.Equals(item, item2))))
                {
                    array[num] = true;
                    flag = true;
                    break;
                }
                num++;
            }
            if (!flag)
            {
                return false;
            }
        }
        return true;
    }

    internal static int GetNextNonEmptyOrWhitespaceIndex(StringSegment input, int startIndex, bool skipEmptyValues, out bool separatorFound)
    {
        separatorFound = false;
        int num = startIndex + HttpRuleParser.GetWhitespaceLength(input, startIndex);
        if (num == input.Length || input[num] != ',')
        {
            return num;
        }
        separatorFound = true;
        num++;
        num += HttpRuleParser.GetWhitespaceLength(input, num);
        if (skipEmptyValues)
        {
            while (num < input.Length && input[num] == ',')
            {
                num++;
                num += HttpRuleParser.GetWhitespaceLength(input, num);
            }
        }
        return num;
    }

    private static int AdvanceCacheDirectiveIndex(int current, string headerValue)
    {
        current += HttpRuleParser.GetWhitespaceLength(headerValue, current);
        if (current < headerValue.Length && headerValue[current] == '=')
        {
            current++;
            current += NameValueHeaderValue.GetValueLength(headerValue, current);
        }
        current = headerValue.IndexOf(',', current);
        if (current == -1)
        {
            return headerValue.Length;
        }
        current++;
        current += HttpRuleParser.GetWhitespaceLength(headerValue, current);
        return current;
    }

    public static bool TryParseSeconds(StringValues headerValues, string targetValue, out TimeSpan? value)
    {
        if (StringValues.IsNullOrEmpty(headerValues) || string.IsNullOrEmpty(targetValue))
        {
            value = null;
            return false;
        }
        for (int i = 0; i < headerValues.Count; i++)
        {
            int num = HttpRuleParser.GetWhitespaceLength(headerValues[i], 0);
            while (num < headerValues[i].Length)
            {
                int num2 = num;
                int tokenLength = HttpRuleParser.GetTokenLength(headerValues[i], num);
                if (tokenLength == targetValue.Length && string.Compare(headerValues[i], num, targetValue, 0, tokenLength, StringComparison.OrdinalIgnoreCase) == 0 && TryParseNonNegativeInt64FromHeaderValue(num + tokenLength, headerValues[i], out var result))
                {
                    value = TimeSpan.FromSeconds(result);
                    return true;
                }
                num = AdvanceCacheDirectiveIndex(num + tokenLength, headerValues[i]);
                if (num <= num2)
                {
                    value = null;
                    return false;
                }
            }
        }
        value = null;
        return false;
    }

    public static bool ContainsCacheDirective(StringValues cacheControlDirectives, string targetDirectives)
    {
        if (StringValues.IsNullOrEmpty(cacheControlDirectives) || string.IsNullOrEmpty(targetDirectives))
        {
            return false;
        }
        for (int i = 0; i < cacheControlDirectives.Count; i++)
        {
            int num = HttpRuleParser.GetWhitespaceLength(cacheControlDirectives[i], 0);
            while (num < cacheControlDirectives[i].Length)
            {
                int num2 = num;
                int tokenLength = HttpRuleParser.GetTokenLength(cacheControlDirectives[i], num);
                if (tokenLength == targetDirectives.Length && string.Compare(cacheControlDirectives[i], num, targetDirectives, 0, tokenLength, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
                num = AdvanceCacheDirectiveIndex(num + tokenLength, cacheControlDirectives[i]);
                if (num <= num2)
                {
                    return false;
                }
            }
        }
        return false;
    }

    private static bool TryParseNonNegativeInt64FromHeaderValue(int startIndex, string headerValue, out long result)
    {
        startIndex += HttpRuleParser.GetWhitespaceLength(headerValue, startIndex);
        if (startIndex >= headerValue.Length - 1 || headerValue[startIndex] != '=')
        {
            result = 0L;
            return false;
        }
        startIndex++;
        startIndex += HttpRuleParser.GetWhitespaceLength(headerValue, startIndex);
        if (TryParseNonNegativeInt64(new StringSegment(headerValue, startIndex, HttpRuleParser.GetNumberLength(headerValue, startIndex, allowDecimal: false)), out result))
        {
            return true;
        }
        result = 0L;
        return false;
    }

    public unsafe static bool TryParseNonNegativeInt32(StringSegment value, out int result)
    {
        if (string.IsNullOrEmpty(value.Buffer) || value.Length == 0)
        {
            result = 0;
            return false;
        }
        result = 0;
        fixed (char* ptr = value.Buffer)
        {
            ushort* ptr2 = (ushort*)(ptr + value.Offset);
            ushort* ptr3 = ptr2 + value.Length;
            ushort num = 0;
            for (; ptr2 < ptr3; ptr2++)
            {
                if ((num = (ushort)(*ptr2 - 48)) > 9)
                {
                    break;
                }
                if ((result = result * 10 + num) < 0)
                {
                    result = 0;
                    return false;
                }
            }
            if (ptr2 != ptr3)
            {
                result = 0;
                return false;
            }
            return true;
        }
    }

    public unsafe static bool TryParseNonNegativeInt64(StringSegment value, out long result)
    {
        if (string.IsNullOrEmpty(value.Buffer) || value.Length == 0)
        {
            result = 0L;
            return false;
        }
        result = 0L;
        fixed (char* ptr = value.Buffer)
        {
            ushort* ptr2 = (ushort*)(ptr + value.Offset);
            ushort* ptr3 = ptr2 + value.Length;
            ushort num = 0;
            for (; ptr2 < ptr3; ptr2++)
            {
                if ((num = (ushort)(*ptr2 - 48)) > 9)
                {
                    break;
                }
                if ((result = result * 10 + num) < 0)
                {
                    result = 0L;
                    return false;
                }
            }
            if (ptr2 != ptr3)
            {
                result = 0L;
                return false;
            }
            return true;
        }
    }

    internal static bool TryParseQualityDouble(StringSegment input, int startIndex, out double quality, out int length)
    {
        quality = 0.0;
        length = 0;
        int length2 = input.Length;
        int num = startIndex;
        int num2 = startIndex + _qualityValueMaxCharCount;
        int num3 = 0;
        int num4 = 0;
        int num5 = 1;
        if (num >= length2)
        {
            return false;
        }
        char c = input[num];
        if (c >= '0' && c <= '1')
        {
            num3 = c - 48;
            num++;
            if (num < length2)
            {
                c = input[num];
                if (c >= '0' && c <= '9')
                {
                    return false;
                }
                if (c == '.')
                {
                    for (num++; num < length2; num++)
                    {
                        c = input[num];
                        if (c < '0' || c > '9')
                        {
                            break;
                        }
                        if (num >= num2)
                        {
                            return false;
                        }
                        num4 = num4 * 10 + c - 48;
                        num5 *= 10;
                    }
                }
            }
            if (num4 != 0)
            {
                quality = (double)num3 + (double)num4 / (double)num5;
            }
            else
            {
                quality = num3;
            }
            if (quality > 1.0)
            {
                quality = 0.0;
                return false;
            }
            length = num - startIndex;
            return true;
        }
        return false;
    }

    public unsafe static string FormatNonNegativeInt64(long value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException("value", value, "The value to be formatted must be non-negative.");
        }
        int num = _int64MaxStringLength;
        char* ptr = stackalloc char[_int64MaxStringLength];
        do
        {
            long num2 = value / 10;
            ptr[--num] = (char)(48 + (value - num2 * 10));
            value = num2;
        }
        while (value != 0L);
        return new string(ptr, num, _int64MaxStringLength - num);
    }

    public static bool TryParseDate(StringSegment input, out DateTimeOffset result)
    {
        return HttpRuleParser.TryStringToDate(input, out result);
    }

    public static string FormatDate(DateTimeOffset dateTime)
    {
        return FormatDate(dateTime, quoted: false);
    }

    public static string FormatDate(DateTimeOffset dateTime, bool quoted)
    {
        return dateTime.ToRfc1123String(quoted);
    }

    public static StringSegment RemoveQuotes(StringSegment input)
    {
        if (IsQuoted(input))
        {
            input = input.Subsegment(1, input.Length - 2);
        }
        return input;
    }

    public static bool IsQuoted(StringSegment input)
    {
        if (!StringSegment.IsNullOrEmpty(input) && input.Length >= 2 && input[0] == '"')
        {
            return input[input.Length - 1] == '"';
        }
        return false;
    }

    public static StringSegment UnescapeAsQuotedString(StringSegment input)
    {
        input = RemoveQuotes(input);
        int num = CountBackslashesForDecodingQuotedString(input);
        if (num == 0)
        {
            return input;
        }
        InplaceStringBuilder val = new InplaceStringBuilder(input.Length - num);
        for (int i = 0; i < input.Length; i++)
        {
            if (i < input.Length - 1 && input[i] == '\\')
            {
                val.Append(input[i + 1]);
                i++;
            }
            else
            {
                val.Append(input[i]);
            }
        }
        return ((object)(InplaceStringBuilder)(val)).ToString();
    }

    private static int CountBackslashesForDecodingQuotedString(StringSegment input)
    {
        int num = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (i < input.Length - 1 && input[i] == '\\')
            {
                if (input[i + 1] == '\\')
                {
                    i++;
                }
                num++;
            }
        }
        return num;
    }

    public static StringSegment EscapeAsQuotedString(StringSegment input)
    {
        int num = CountAndCheckCharactersNeedingBackslashesWhenEncoding(input);
        InplaceStringBuilder val = new InplaceStringBuilder(input.Length + num + 2);
        val.Append('"');
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\' || input[i] == '"')
            {
                val.Append('\\');
            }
            else if ((input[i] <= '\u001f' || input[i] == '\u007f') && input[i] != '\t')
            {
                throw new FormatException($"Invalid control character '{input[i]}' in input.");
            }
            val.Append(input[i]);
        }
        val.Append('"');
        return ((object)(InplaceStringBuilder)(val)).ToString();
    }

    private static int CountAndCheckCharactersNeedingBackslashesWhenEncoding(StringSegment input)
    {
        int num = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\' || input[i] == '"')
            {
                num++;
            }
        }
        return num;
    }

    internal static void ThrowIfReadOnly(bool isReadOnly)
    {
        if (isReadOnly)
        {
            throw new InvalidOperationException("The object cannot be modified because it is read-only.");
        }
    }
}
