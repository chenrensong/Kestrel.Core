using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.AspNetCore.Http.Authentication;

public class AuthenticationDescription
{
	private const string DisplayNamePropertyKey = "DisplayName";

	private const string AuthenticationSchemePropertyKey = "AuthenticationScheme";

	public IDictionary<string, object> Items { get; }

	public string AuthenticationScheme
	{
		get
		{
			return GetString("AuthenticationScheme");
		}
		set
		{
			Items["AuthenticationScheme"] = value;
		}
	}

	public string DisplayName
	{
		get
		{
			return GetString("DisplayName");
		}
		set
		{
			Items["DisplayName"] = value;
		}
	}

	public AuthenticationDescription()
		: this(null)
	{
	}

	public AuthenticationDescription(IDictionary<string, object> items)
	{
		Items = items ?? new Dictionary<string, object>(StringComparer.Ordinal);
	}

	private string GetString(string name)
	{
		if (Items.TryGetValue(name, out var value))
		{
			return Convert.ToString(value, CultureInfo.InvariantCulture);
		}
		return null;
	}
}
