using System.Collections.Generic;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Connections;

public abstract class ConnectionContext
{
	public abstract string ConnectionId { get; set; }

	public abstract IFeatureCollection Features { get; }

	public abstract IDictionary<object, object> Items { get; set; }

	public abstract IDuplexPipe Transport { get; set; }

	public virtual void Abort(ConnectionAbortedException abortReason)
	{
		Features.Get<IConnectionLifetimeFeature>()?.Abort();
	}

	public virtual void Abort()
	{
		Abort(new ConnectionAbortedException("The connection was aborted by the application."));
	}
}
