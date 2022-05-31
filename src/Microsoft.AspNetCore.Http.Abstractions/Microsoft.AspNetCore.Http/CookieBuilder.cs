using System;
using Microsoft.AspNetCore.Http.Abstractions;

namespace Microsoft.AspNetCore.Http;

public class CookieBuilder
{
	private string _name;

	public virtual string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, "value");
			}
			_name = value;
		}
	}

	public virtual string Path { get; set; }

	public virtual string Domain { get; set; }

	public virtual bool HttpOnly { get; set; }

	public virtual SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;


	public virtual CookieSecurePolicy SecurePolicy { get; set; }

	public virtual TimeSpan? Expiration { get; set; }

	public virtual TimeSpan? MaxAge { get; set; }

	public virtual bool IsEssential { get; set; }

	public CookieOptions Build(HttpContext context)
	{
		return Build(context, DateTimeOffset.Now);
	}

	public virtual CookieOptions Build(HttpContext context, DateTimeOffset expiresFrom)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return new CookieOptions
		{
			Path = (Path ?? "/"),
			SameSite = SameSite,
			HttpOnly = HttpOnly,
			MaxAge = MaxAge,
			Domain = Domain,
			IsEssential = IsEssential,
			Secure = (SecurePolicy == CookieSecurePolicy.Always || (SecurePolicy == CookieSecurePolicy.SameAsRequest && context.Request.IsHttps)),
			Expires = (Expiration.HasValue ? new DateTimeOffset?(expiresFrom.Add(Expiration.Value)) : null)
		};
	}
}
