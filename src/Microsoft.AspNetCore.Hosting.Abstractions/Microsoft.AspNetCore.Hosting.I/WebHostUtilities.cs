using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class WebHostUtilities
{
	public static bool ParseBool(IConfiguration configuration, string key)
	{
		if (!string.Equals("true", configuration[key], StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals("1", configuration[key], StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
