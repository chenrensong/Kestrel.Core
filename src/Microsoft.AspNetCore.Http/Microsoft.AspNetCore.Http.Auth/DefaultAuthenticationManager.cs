using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Microsoft.AspNetCore.Http.Authentication.Internal;

[Obsolete("This is obsolete and will be removed in a future version. See https://go.microsoft.com/fwlink/?linkid=845470.")]
public class DefaultAuthenticationManager : AuthenticationManager
{
	private static readonly Func<IFeatureCollection, IHttpAuthenticationFeature> _newAuthenticationFeature = (IFeatureCollection f) => new HttpAuthenticationFeature();

	private HttpContext _context;

	private FeatureReferences<IHttpAuthenticationFeature> _features;

	public override HttpContext HttpContext => _context;

	private IHttpAuthenticationFeature HttpAuthenticationFeature => _features.Fetch(ref _features.Cache, _newAuthenticationFeature);

	public DefaultAuthenticationManager(HttpContext context)
	{
		Initialize(context);
	}

	public virtual void Initialize(HttpContext context)
	{
		_context = context;
		_features = new FeatureReferences<IHttpAuthenticationFeature>(context.Features);
	}

	public virtual void Uninitialize()
	{
		_features = default(FeatureReferences<IHttpAuthenticationFeature>);
	}

	public override IEnumerable<AuthenticationDescription> GetAuthenticationSchemes()
	{
		IAuthenticationHandler handler = HttpAuthenticationFeature.Handler;
		if (handler == null)
		{
			return new AuthenticationDescription[0];
		}
		DescribeSchemesContext describeSchemesContext = new DescribeSchemesContext();
		handler.GetDescriptions(describeSchemesContext);
		return describeSchemesContext.Results.Select((IDictionary<string, object> description) => new AuthenticationDescription(description));
	}

	public override async Task AuthenticateAsync(AuthenticateContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		IAuthenticationHandler handler = HttpAuthenticationFeature.Handler;
		if (handler != null)
		{
			await handler.AuthenticateAsync(context);
		}
		if (!context.Accepted)
		{
			throw new InvalidOperationException("No authentication handler is configured to authenticate for the scheme: " + context.AuthenticationScheme);
		}
	}

	public override async Task<AuthenticateInfo> GetAuthenticateInfoAsync(string authenticationScheme)
	{
		if (authenticationScheme == null)
		{
			throw new ArgumentNullException("authenticationScheme");
		}
		IAuthenticationHandler handler = HttpAuthenticationFeature.Handler;
		AuthenticateContext context = new AuthenticateContext(authenticationScheme);
		if (handler != null)
		{
			await handler.AuthenticateAsync(context);
		}
		if (!context.Accepted)
		{
			throw new InvalidOperationException("No authentication handler is configured to authenticate for the scheme: " + context.AuthenticationScheme);
		}
		return new AuthenticateInfo
		{
			Principal = context.Principal,
			Properties = new AuthenticationProperties(context.Properties),
			Description = new AuthenticationDescription(context.Description)
		};
	}

	public override async Task ChallengeAsync(string authenticationScheme, AuthenticationProperties properties, ChallengeBehavior behavior)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		IAuthenticationHandler handler = HttpAuthenticationFeature.Handler;
		ChallengeContext challengeContext = new ChallengeContext(authenticationScheme, properties?.Items, behavior);
		if (handler != null)
		{
			await handler.ChallengeAsync(challengeContext);
		}
		if (!challengeContext.Accepted)
		{
			throw new InvalidOperationException("No authentication handler is configured to handle the scheme: " + authenticationScheme);
		}
	}

	public override async Task SignInAsync(string authenticationScheme, ClaimsPrincipal principal, AuthenticationProperties properties)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		if (principal == null)
		{
			throw new ArgumentNullException("principal");
		}
		IAuthenticationHandler handler = HttpAuthenticationFeature.Handler;
		SignInContext signInContext = new SignInContext(authenticationScheme, principal, properties?.Items);
		if (handler != null)
		{
			await handler.SignInAsync(signInContext);
		}
		if (!signInContext.Accepted)
		{
			throw new InvalidOperationException("No authentication handler is configured to handle the scheme: " + authenticationScheme);
		}
	}

	public override async Task SignOutAsync(string authenticationScheme, AuthenticationProperties properties)
	{
		if (string.IsNullOrEmpty(authenticationScheme))
		{
			throw new ArgumentException("authenticationScheme");
		}
		IAuthenticationHandler handler = HttpAuthenticationFeature.Handler;
		SignOutContext signOutContext = new SignOutContext(authenticationScheme, properties?.Items);
		if (handler != null)
		{
			await handler.SignOutAsync(signOutContext);
		}
		if (!signOutContext.Accepted)
		{
			throw new InvalidOperationException("No authentication handler is configured to handle the scheme: " + authenticationScheme);
		}
	}
}
