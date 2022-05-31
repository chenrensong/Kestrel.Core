using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public abstract class HttpRequest
{
	public abstract HttpContext HttpContext { get; }

	public abstract string Method { get; set; }

	public abstract string Scheme { get; set; }

	public abstract bool IsHttps { get; set; }

	public abstract HostString Host { get; set; }

	public abstract PathString PathBase { get; set; }

	public abstract PathString Path { get; set; }

	public abstract QueryString QueryString { get; set; }

	public abstract IQueryCollection Query { get; set; }

	public abstract string Protocol { get; set; }

	public abstract IHeaderDictionary Headers { get; }

	public abstract IRequestCookieCollection Cookies { get; set; }

	public abstract long? ContentLength { get; set; }

	public abstract string ContentType { get; set; }

	public abstract Stream Body { get; set; }

	public abstract bool HasFormContentType { get; }

	public abstract IFormCollection Form { get; set; }

	public abstract Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default(CancellationToken));
}
