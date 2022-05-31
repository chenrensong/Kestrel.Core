using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Http.Features.Authentication;

public class ChallengeContext
{
	public string AuthenticationScheme { get; }

	public ChallengeBehavior Behavior { get; }

	public IDictionary<string, string> Properties { get; }

	public bool Accepted { get; private set; }

	public ChallengeContext(string authenticationScheme)
		: this(authenticationScheme, null, ChallengeBehavior.Automatic)
	{
	}

	public ChallengeContext(string authenticationScheme, IDictionary<string, string> properties, ChallengeBehavior behavior)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		AuthenticationScheme = authenticationScheme;
		Properties = properties ?? new Dictionary<string, string>(StringComparer.Ordinal);
		Behavior = behavior;
	}

	public void Accept()
	{
		Accepted = true;
	}
}
