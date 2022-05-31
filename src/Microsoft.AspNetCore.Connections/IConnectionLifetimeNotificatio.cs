using System.Threading;

namespace Microsoft.AspNetCore.Connections.Features;

public interface IConnectionLifetimeNotificationFeature
{
	CancellationToken ConnectionClosedRequested { get; set; }

	void RequestClose();
}
