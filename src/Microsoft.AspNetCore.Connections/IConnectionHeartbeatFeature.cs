using System;

namespace Microsoft.AspNetCore.Connections.Features;

public interface IConnectionHeartbeatFeature
{
	void OnHeartbeat(Action<object> action, object state);
}
