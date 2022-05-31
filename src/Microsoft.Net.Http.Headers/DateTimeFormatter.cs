using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

internal static class DateTimeFormatter
{
    private static readonly DateTimeFormatInfo FormatInfo = CultureInfo.InvariantCulture.DateTimeFormat;

    private static readonly string[] MonthNames = FormatInfo.AbbreviatedMonthNames;

    private static readonly string[] DayNames = FormatInfo.AbbreviatedDayNames;

    private static readonly int Rfc1123DateLength = "ddd, dd MMM yyyy HH:mm:ss GMT".Length;

    private static readonly int QuotedRfc1123DateLength = Rfc1123DateLength + 2;

    private const int AsciiNumberOffset = 48;

    private const string Gmt = "GMT";

    private const char Comma = ',';

    private const char Space = ' ';

    private const char Colon = ':';

    private const char Quote = '"';

    public static string ToRfc1123String(this DateTimeOffset dateTime)
    {
        return dateTime.ToRfc1123String(quoted: false);
    }

    public static string ToRfc1123String(this DateTimeOffset dateTime, bool quoted)
    {
        DateTime utcDateTime = dateTime.UtcDateTime;
        int num = (quoted ? QuotedRfc1123DateLength : Rfc1123DateLength);
        InplaceStringBuilder target = new InplaceStringBuilder(num);
        if (quoted)
        {
            target.Append('"');
        }
        target.Append(DayNames[(int)utcDateTime.DayOfWeek]);
        target.Append(',');
        target.Append(' ');
        AppendNumber(ref target, utcDateTime.Day);
        target.Append(' ');
        target.Append(MonthNames[utcDateTime.Month - 1]);
        target.Append(' ');
        AppendYear(ref target, utcDateTime.Year);
        target.Append(' ');
        AppendTimeOfDay(ref target, utcDateTime.TimeOfDay);
        target.Append(' ');
        target.Append("GMT");
        if (quoted)
        {
            target.Append('"');
        }
        return target.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendYear(ref InplaceStringBuilder target, int year)
    {
        target.Append(GetAsciiChar(year / 1000));
        target.Append(GetAsciiChar(year % 1000 / 100));
        target.Append(GetAsciiChar(year % 100 / 10));
        target.Append(GetAsciiChar(year % 10));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendTimeOfDay(ref InplaceStringBuilder target, TimeSpan timeOfDay)
    {
        AppendNumber(ref target, timeOfDay.Hours);
        target.Append(':');
        AppendNumber(ref target, timeOfDay.Minutes);
        target.Append(':');
        AppendNumber(ref target, timeOfDay.Seconds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendNumber(ref InplaceStringBuilder target, int number)
    {
        target.Append(GetAsciiChar(number / 10));
        target.Append(GetAsciiChar(number % 10));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char GetAsciiChar(int value)
    {
        return (char)(48 + value);
    }
}
