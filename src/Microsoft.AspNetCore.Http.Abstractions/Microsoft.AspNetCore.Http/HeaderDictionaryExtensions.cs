using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http;

public static class HeaderDictionaryExtensions
{
	public static void Append(this IHeaderDictionary headers, string key, StringValues value)
	{
		ParsingHelpers.AppendHeaderUnmodified(headers, key, value);
	}

	public static void AppendCommaSeparatedValues(this IHeaderDictionary headers, string key, params string[] values)
	{
		ParsingHelpers.AppendHeaderJoined(headers, key, values);
	}

	public static string[] GetCommaSeparatedValues(this IHeaderDictionary headers, string key)
	{
		return ParsingHelpers.GetHeaderSplit(headers, key).ToArray();
	}

	public static void SetCommaSeparatedValues(this IHeaderDictionary headers, string key, params string[] values)
	{
		ParsingHelpers.SetHeaderJoined(headers, key, values);
	}
}
