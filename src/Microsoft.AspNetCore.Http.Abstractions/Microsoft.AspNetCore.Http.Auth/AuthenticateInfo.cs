using System.Security.Claims;

namespace Microsoft.AspNetCore.Http.Authentication;

public class AuthenticateInfo
{
	public ClaimsPrincipal Principal { get; set; }

	public AuthenticationProperties Properties { get; set; }

	public AuthenticationDescription Description { get; set; }
}
