using System.Threading;

namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpRequestLifetimeFeature
{
	CancellationToken RequestAborted { get; set; }

	void Abort();
}
