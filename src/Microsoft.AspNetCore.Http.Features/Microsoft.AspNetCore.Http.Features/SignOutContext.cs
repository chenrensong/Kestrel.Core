using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Http.Features.Authentication;

public class SignOutContext
{
	public string AuthenticationScheme { get; }

	public IDictionary<string, string> Properties { get; }

	public bool Accepted { get; private set; }

	public SignOutContext(string authenticationScheme, IDictionary<string, string> properties)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		AuthenticationScheme = authenticationScheme;
		Properties = properties ?? new Dictionary<string, string>(StringComparer.Ordinal);
	}

	public void Accept()
	{
		Accepted = true;
	}
}
