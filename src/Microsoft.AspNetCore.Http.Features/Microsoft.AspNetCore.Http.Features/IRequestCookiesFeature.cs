namespace Microsoft.AspNetCore.Http.Features;

public interface IRequestCookiesFeature
{
	IRequestCookieCollection Cookies { get; set; }
}
