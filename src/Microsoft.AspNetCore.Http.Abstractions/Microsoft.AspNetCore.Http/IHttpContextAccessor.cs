namespace Microsoft.AspNetCore.Http;

public interface IHttpContextAccessor
{
	HttpContext HttpContext { get; set; }
}
