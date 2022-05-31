using System;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Http.Features.Authentication;

public interface IHttpAuthenticationFeature
{
	ClaimsPrincipal User { get; set; }

	[Obsolete("This is obsolete and will be removed in a future version. See https://go.microsoft.com/fwlink/?linkid=845470.")]
	IAuthenticationHandler Handler { get; set; }
}
