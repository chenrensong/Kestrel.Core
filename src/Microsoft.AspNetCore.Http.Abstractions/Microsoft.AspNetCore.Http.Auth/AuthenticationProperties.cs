using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.AspNetCore.Http.Authentication;

public class AuthenticationProperties
{
	internal const string IssuedUtcKey = ".issued";

	internal const string ExpiresUtcKey = ".expires";

	internal const string IsPersistentKey = ".persistent";

	internal const string RedirectUriKey = ".redirect";

	internal const string RefreshKey = ".refresh";

	internal const string UtcDateTimeFormat = "r";

	public IDictionary<string, string> Items { get; }

	public bool IsPersistent
	{
		get
		{
			return Items.ContainsKey(".persistent");
		}
		set
		{
			if (Items.ContainsKey(".persistent"))
			{
				if (!value)
				{
					Items.Remove(".persistent");
				}
			}
			else if (value)
			{
				Items.Add(".persistent", string.Empty);
			}
		}
	}

	public string RedirectUri
	{
		get
		{
			if (!Items.TryGetValue(".redirect", out var value))
			{
				return null;
			}
			return value;
		}
		set
		{
			if (value != null)
			{
				Items[".redirect"] = value;
			}
			else if (Items.ContainsKey(".redirect"))
			{
				Items.Remove(".redirect");
			}
		}
	}

	public DateTimeOffset? IssuedUtc
	{
		get
		{
			if (Items.TryGetValue(".issued", out var value) && DateTimeOffset.TryParseExact(value, "r", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			if (value.HasValue)
			{
				Items[".issued"] = value.Value.ToString("r", CultureInfo.InvariantCulture);
			}
			else if (Items.ContainsKey(".issued"))
			{
				Items.Remove(".issued");
			}
		}
	}

	public DateTimeOffset? ExpiresUtc
	{
		get
		{
			if (Items.TryGetValue(".expires", out var value) && DateTimeOffset.TryParseExact(value, "r", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			if (value.HasValue)
			{
				Items[".expires"] = value.Value.ToString("r", CultureInfo.InvariantCulture);
			}
			else if (Items.ContainsKey(".expires"))
			{
				Items.Remove(".expires");
			}
		}
	}

	public bool? AllowRefresh
	{
		get
		{
			if (Items.TryGetValue(".refresh", out var value) && bool.TryParse(value, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			if (value.HasValue)
			{
				Items[".refresh"] = value.Value.ToString();
			}
			else if (Items.ContainsKey(".refresh"))
			{
				Items.Remove(".refresh");
			}
		}
	}

	public AuthenticationProperties()
		: this(null)
	{
	}

	public AuthenticationProperties(IDictionary<string, string> items)
	{
		Items = items ?? new Dictionary<string, string>(StringComparer.Ordinal);
	}
}
