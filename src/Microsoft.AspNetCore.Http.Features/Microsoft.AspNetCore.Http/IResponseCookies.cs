namespace Microsoft.AspNetCore.Http;

public interface IResponseCookies
{
	void Append(string key, string value);

	void Append(string key, string value, CookieOptions options);

	void Delete(string key);

	void Delete(string key, CookieOptions options);
}
