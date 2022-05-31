using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Microsoft.AspNetCore.Http.Authentication;

[Obsolete("This is obsolete and will be removed in a future version. See https://go.microsoft.com/fwlink/?linkid=845470.")]
public abstract class AuthenticationManager
{
	public const string AutomaticScheme = "Automatic";

	public abstract HttpContext HttpContext { get; }

	public abstract IEnumerable<AuthenticationDescription> GetAuthenticationSchemes();

	public abstract Task<AuthenticateInfo> GetAuthenticateInfoAsync(string authenticationScheme);

	public abstract Task AuthenticateAsync(AuthenticateContext context);

	public virtual async Task<ClaimsPrincipal> AuthenticateAsync(string authenticationScheme)
	{
		return (await GetAuthenticateInfoAsync(authenticationScheme))?.Principal;
	}

	public virtual Task ChallengeAsync()
	{
		return ChallengeAsync((AuthenticationProperties)null);
	}

	public virtual Task ChallengeAsync(AuthenticationProperties properties)
	{
		return ChallengeAsync("Automatic", properties);
	}

	public virtual Task ChallengeAsync(string authenticationScheme)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		return ChallengeAsync(authenticationScheme, null);
	}

	public virtual Task ChallengeAsync(string authenticationScheme, AuthenticationProperties properties)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		return ChallengeAsync(authenticationScheme, properties, ChallengeBehavior.Automatic);
	}

	public virtual Task SignInAsync(string authenticationScheme, ClaimsPrincipal principal)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		if (principal == null)
		{
			throw new ArgumentNullException("principal");
		}
		return SignInAsync(authenticationScheme, principal, null);
	}

	public virtual Task ForbidAsync()
	{
		return ForbidAsync("Automatic", null);
	}

	public virtual Task ForbidAsync(string authenticationScheme)
	{
		if (authenticationScheme == null)
		{
			throw new ArgumentNullException("authenticationScheme");
		}
		return ForbidAsync(authenticationScheme, null);
	}

	public virtual Task ForbidAsync(string authenticationScheme, AuthenticationProperties properties)
	{
		if (authenticationScheme == null)
		{
			throw new ArgumentNullException("authenticationScheme");
		}
		return ChallengeAsync(authenticationScheme, properties, ChallengeBehavior.Forbidden);
	}

	public virtual Task ForbidAsync(AuthenticationProperties properties)
	{
		return ForbidAsync("Automatic", properties);
	}

	public abstract Task ChallengeAsync(string authenticationScheme, AuthenticationProperties properties, ChallengeBehavior behavior);

	public abstract Task SignInAsync(string authenticationScheme, ClaimsPrincipal principal, AuthenticationProperties properties);

	public virtual Task SignOutAsync(string authenticationScheme)
	{
		if (authenticationScheme == null)
		{
			throw new ArgumentNullException("authenticationScheme");
		}
		return SignOutAsync(authenticationScheme, null);
	}

	public abstract Task SignOutAsync(string authenticationScheme, AuthenticationProperties properties);
}
