namespace Microsoft.AspNetCore.Http.Internal;

internal class HostStringHelper
{
	private static bool[] SafeHostStringChars = new bool[128]
	{
		false, false, false, false, false, false, false, false, false, false,
		false, false, false, false, false, false, false, false, false, false,
		false, false, false, false, false, false, false, false, false, false,
		false, false, false, false, false, false, false, true, false, false,
		false, false, false, false, false, true, true, false, true, true,
		true, true, true, true, true, true, true, true, true, false,
		false, false, false, false, false, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, false, true, false, false, false, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, false, false, false, false, false
	};

	public static bool IsSafeHostStringChar(char c)
	{
		if (c < SafeHostStringChars.Length)
		{
			return SafeHostStringChars[(uint)c];
		}
		return false;
	}
}
