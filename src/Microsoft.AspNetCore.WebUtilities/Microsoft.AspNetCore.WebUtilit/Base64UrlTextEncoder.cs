namespace Microsoft.AspNetCore.WebUtilities;

public static class Base64UrlTextEncoder
{
	public static string Encode(byte[] data)
	{
		return WebEncoders.Base64UrlEncode(data);
	}

	public static byte[] Decode(string text)
	{
		return WebEncoders.Base64UrlDecode(text);
	}
}
