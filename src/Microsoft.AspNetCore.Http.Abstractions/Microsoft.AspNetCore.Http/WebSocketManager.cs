using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public abstract class WebSocketManager
{
	public abstract bool IsWebSocketRequest { get; }

	public abstract IList<string> WebSocketRequestedProtocols { get; }

	public virtual Task<WebSocket> AcceptWebSocketAsync()
	{
		return AcceptWebSocketAsync(null);
	}

	public abstract Task<WebSocket> AcceptWebSocketAsync(string subProtocol);
}
