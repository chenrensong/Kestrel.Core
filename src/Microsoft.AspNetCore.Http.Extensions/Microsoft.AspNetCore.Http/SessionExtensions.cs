using System.Text;

namespace Microsoft.AspNetCore.Http;

public static class SessionExtensions
{
	public static void SetInt32(this ISession session, string key, int value)
	{
		byte[] value2 = new byte[4]
		{
			(byte)(value >> 24),
			(byte)(0xFFu & (uint)(value >> 16)),
			(byte)(0xFFu & (uint)(value >> 8)),
			(byte)(0xFFu & (uint)value)
		};
		session.Set(key, value2);
	}

	public static int? GetInt32(this ISession session, string key)
	{
		byte[] array = session.Get(key);
		if (array == null || array.Length < 4)
		{
			return null;
		}
		return (array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3];
	}

	public static void SetString(this ISession session, string key, string value)
	{
		session.Set(key, Encoding.UTF8.GetBytes(value));
	}

	public static string GetString(this ISession session, string key)
	{
		byte[] array = session.Get(key);
		if (array == null)
		{
			return null;
		}
		return Encoding.UTF8.GetString(array);
	}

	public static byte[] Get(this ISession session, string key)
	{
		byte[] value = null;
		session.TryGetValue(key, out value);
		return value;
	}
}
