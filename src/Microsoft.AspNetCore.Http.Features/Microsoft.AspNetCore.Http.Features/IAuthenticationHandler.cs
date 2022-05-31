using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Features.Authentication;

public interface IAuthenticationHandler
{
	void GetDescriptions(DescribeSchemesContext context);

	Task AuthenticateAsync(AuthenticateContext context);

	Task ChallengeAsync(ChallengeContext context);

	Task SignInAsync(SignInContext context);

	Task SignOutAsync(SignOutContext context);
}
