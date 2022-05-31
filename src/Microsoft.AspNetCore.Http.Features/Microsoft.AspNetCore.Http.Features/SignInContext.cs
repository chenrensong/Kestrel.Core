using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Http.Features.Authentication;

public class SignInContext
{
	public string AuthenticationScheme { get; }

	public ClaimsPrincipal Principal { get; }

	public IDictionary<string, string> Properties { get; }

	public bool Accepted { get; private set; }

	public SignInContext(string authenticationScheme, ClaimsPrincipal principal, IDictionary<string, string> properties)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		if (principal == null)
		{
			throw new ArgumentNullException("principal");
		}
		AuthenticationScheme = authenticationScheme;
		Principal = principal;
		Properties = properties ?? new Dictionary<string, string>(StringComparer.Ordinal);
	}

	public void Accept()
	{
		Accepted = true;
	}
}
