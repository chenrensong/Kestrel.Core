using System;
using System.Globalization;

namespace Microsoft.AspNetCore.Http.Internal;

public class BindingAddress
{
	public string Host { get; private set; }

	public string PathBase { get; private set; }

	public int Port { get; internal set; }

	public string Scheme { get; private set; }

	public bool IsUnixPipe => Host.StartsWith("unix:/", StringComparison.Ordinal);

	public string UnixPipePath
	{
		get
		{
			if (!IsUnixPipe)
			{
				throw new InvalidOperationException("Binding address is not a unix pipe.");
			}
			return Host.Substring("unix:/".Length - 1);
		}
	}

	public override string ToString()
	{
		if (IsUnixPipe)
		{
			return Scheme.ToLowerInvariant() + "://" + Host.ToLowerInvariant();
		}
		return Scheme.ToLowerInvariant() + "://" + Host.ToLowerInvariant() + ":" + Port.ToString(CultureInfo.InvariantCulture) + PathBase.ToString(CultureInfo.InvariantCulture);
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BindingAddress bindingAddress))
		{
			return false;
		}
		if (string.Equals(Scheme, bindingAddress.Scheme, StringComparison.OrdinalIgnoreCase) && string.Equals(Host, bindingAddress.Host, StringComparison.OrdinalIgnoreCase) && Port == bindingAddress.Port)
		{
			return PathBase == bindingAddress.PathBase;
		}
		return false;
	}

	public static BindingAddress Parse(string address)
	{
		address = address ?? string.Empty;
		int num = address.IndexOf("://", StringComparison.Ordinal);
		if (num < 0)
		{
			throw new FormatException("Invalid url: '" + address + "'");
		}
		int num2 = num + "://".Length;
		bool num3 = address.IndexOf("unix:/", num2, StringComparison.Ordinal) == num2;
		int num4;
		int num5;
		if (!num3)
		{
			num4 = address.IndexOf("/", num2, StringComparison.Ordinal);
			num5 = num4;
		}
		else
		{
			num4 = address.IndexOf(":", num2 + "unix:/".Length, StringComparison.Ordinal);
			num5 = num4 + ":".Length;
		}
		if (num4 < 0)
		{
			num4 = (num5 = address.Length);
		}
		BindingAddress bindingAddress = new BindingAddress
		{
			Scheme = address.Substring(0, num)
		};
		bool flag = false;
		if (!num3)
		{
			int num6 = address.LastIndexOf(":", num4 - 1, num4 - num2, StringComparison.Ordinal);
			if (num6 >= 0)
			{
				int num7 = num6 + ":".Length;
				if (int.TryParse(address.Substring(num7, num4 - num7), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
				{
					flag = true;
					bindingAddress.Host = address.Substring(num2, num6 - num2);
					bindingAddress.Port = result;
				}
			}
			if (!flag)
			{
				if (string.Equals(bindingAddress.Scheme, "http", StringComparison.OrdinalIgnoreCase))
				{
					bindingAddress.Port = 80;
				}
				else if (string.Equals(bindingAddress.Scheme, "https", StringComparison.OrdinalIgnoreCase))
				{
					bindingAddress.Port = 443;
				}
			}
		}
		if (!flag)
		{
			bindingAddress.Host = address.Substring(num2, num4 - num2);
		}
		if (string.IsNullOrEmpty(bindingAddress.Host))
		{
			throw new FormatException("Invalid url: '" + address + "'");
		}
		if (address[address.Length - 1] == '/')
		{
			bindingAddress.PathBase = address.Substring(num5, address.Length - num5 - 1);
		}
		else
		{
			bindingAddress.PathBase = address.Substring(num5);
		}
		return bindingAddress;
	}
}
