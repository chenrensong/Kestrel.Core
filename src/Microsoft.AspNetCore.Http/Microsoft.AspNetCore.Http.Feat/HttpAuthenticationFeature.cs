using System.Security.Claims;

namespace Microsoft.AspNetCore.Http.Features.Authentication;

public class HttpAuthenticationFeature : IHttpAuthenticationFeature
{
	public ClaimsPrincipal User { get; set; }

	public IAuthenticationHandler Handler { get; set; }
}
