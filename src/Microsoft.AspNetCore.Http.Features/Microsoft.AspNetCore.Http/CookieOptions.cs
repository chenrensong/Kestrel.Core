using System;

namespace Microsoft.AspNetCore.Http;

public class CookieOptions
{
	public string Domain { get; set; }

	public string Path { get; set; }

	public DateTimeOffset? Expires { get; set; }

	public bool Secure { get; set; }

	public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;


	public bool HttpOnly { get; set; }

	public TimeSpan? MaxAge { get; set; }

	public bool IsEssential { get; set; }

	public CookieOptions()
	{
		Path = "/";
	}
}
