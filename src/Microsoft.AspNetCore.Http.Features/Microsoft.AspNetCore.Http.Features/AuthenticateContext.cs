using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Http.Features.Authentication;

public class AuthenticateContext
{
	public string AuthenticationScheme { get; }

	public bool Accepted { get; private set; }

	public ClaimsPrincipal Principal { get; private set; }

	public IDictionary<string, string> Properties { get; private set; }

	public IDictionary<string, object> Description { get; private set; }

	public Exception Error { get; private set; }

	public AuthenticateContext(string authenticationScheme)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		AuthenticationScheme = authenticationScheme;
	}

	public virtual void Authenticated(ClaimsPrincipal principal, IDictionary<string, string> properties, IDictionary<string, object> description)
	{
		Accepted = true;
		Principal = principal;
		Properties = properties;
		Description = description;
		Error = null;
	}

	public virtual void NotAuthenticated()
	{
		Accepted = true;
		Description = null;
		Error = null;
		Principal = null;
		Properties = null;
	}

	public virtual void Failed(Exception error)
	{
		Accepted = true;
		Error = error;
		Description = null;
		Principal = null;
		Properties = null;
	}
}
