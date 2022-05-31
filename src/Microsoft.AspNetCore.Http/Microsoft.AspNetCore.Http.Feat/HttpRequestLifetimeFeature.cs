using System.Threading;

namespace Microsoft.AspNetCore.Http.Features;

public class HttpRequestLifetimeFeature : IHttpRequestLifetimeFeature
{
	public CancellationToken RequestAborted { get; set; }

	public void Abort()
	{
	}
}
