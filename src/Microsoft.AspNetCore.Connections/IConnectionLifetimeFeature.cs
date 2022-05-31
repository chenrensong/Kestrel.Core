using System.Threading;

namespace Microsoft.AspNetCore.Connections.Features;

public interface IConnectionLifetimeFeature
{
	CancellationToken ConnectionClosed { get; set; }

	void Abort();
}
