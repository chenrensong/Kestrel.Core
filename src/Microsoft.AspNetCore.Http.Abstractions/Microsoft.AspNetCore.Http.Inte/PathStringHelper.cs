namespace Microsoft.AspNetCore.Http.Internal;

internal class PathStringHelper
{
	private static bool[] ValidPathChars = new bool[128]
	{
		false, false, false, false, false, false, false, false, false, false,
		false, false, false, false, false, false, false, false, false, false,
		false, false, false, false, false, false, false, false, false, false,
		false, false, false, true, false, false, true, false, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		false, true, false, false, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, false, false, false, false, true, false, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, false, false, false, true, false
	};

	public static bool IsValidPathChar(char c)
	{
		if (c < ValidPathChars.Length)
		{
			return ValidPathChars[(uint)c];
		}
		return false;
	}

	public static bool IsPercentEncodedChar(string str, int index)
	{
		if (index < str.Length - 2 && str[index] == '%' && IsHexadecimalChar(str[index + 1]))
		{
			return IsHexadecimalChar(str[index + 2]);
		}
		return false;
	}

	public static bool IsHexadecimalChar(char c)
	{
		if (('0' > c || c > '9') && ('A' > c || c > 'F'))
		{
			if ('a' <= c)
			{
				return c <= 'f';
			}
			return false;
		}
		return true;
	}
}
