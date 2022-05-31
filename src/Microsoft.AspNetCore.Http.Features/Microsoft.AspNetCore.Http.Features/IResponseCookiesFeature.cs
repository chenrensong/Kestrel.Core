namespace Microsoft.AspNetCore.Http.Features;

public interface IResponseCookiesFeature
{
	IResponseCookies Cookies { get; }
}
