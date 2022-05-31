using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpWebSocketFeature
{
	bool IsWebSocketRequest { get; }

	Task<WebSocket> AcceptAsync(WebSocketAcceptContext context);
}
