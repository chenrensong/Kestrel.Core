using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public interface IMiddleware
{
	Task InvokeAsync(HttpContext context, RequestDelegate next);
}
