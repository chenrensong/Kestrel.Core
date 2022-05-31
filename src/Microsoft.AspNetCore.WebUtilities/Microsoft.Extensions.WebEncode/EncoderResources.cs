using System.Globalization;

namespace Microsoft.Extensions.WebEncoders.Sources;

internal static class EncoderResources
{
	internal static readonly string WebEncoders_InvalidCountOffsetOrLength = "Invalid {0}, {1} or {2} length.";

	internal static readonly string WebEncoders_MalformedInput = "Malformed input: {0} is an invalid input length.";

	internal static string FormatWebEncoders_InvalidCountOffsetOrLength(object p0, object p1, object p2)
	{
		return string.Format(CultureInfo.CurrentCulture, WebEncoders_InvalidCountOffsetOrLength, p0, p1, p2);
	}

	internal static string FormatWebEncoders_MalformedInput(object p0)
	{
		return string.Format(CultureInfo.CurrentCulture, WebEncoders_MalformedInput, p0);
	}
}
