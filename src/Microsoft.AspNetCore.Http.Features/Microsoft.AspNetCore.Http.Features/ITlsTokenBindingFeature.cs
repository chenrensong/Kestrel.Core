namespace Microsoft.AspNetCore.Http.Features;

public interface ITlsTokenBindingFeature
{
	byte[] GetProvidedTokenBindingId();

	byte[] GetReferredTokenBindingId();
}
