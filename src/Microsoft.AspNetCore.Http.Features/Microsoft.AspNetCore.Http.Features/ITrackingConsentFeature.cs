namespace Microsoft.AspNetCore.Http.Features;

public interface ITrackingConsentFeature
{
	bool IsConsentNeeded { get; }

	bool HasConsent { get; }

	bool CanTrack { get; }

	void GrantConsent();

	void WithdrawConsent();

	string CreateConsentCookie();
}
