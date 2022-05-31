using System.Net;

namespace Microsoft.AspNetCore.Http.Features;

public class HttpConnectionFeature : IHttpConnectionFeature
{
	public string ConnectionId { get; set; }

	public IPAddress LocalIpAddress { get; set; }

	public int LocalPort { get; set; }

	public IPAddress RemoteIpAddress { get; set; }

	public int RemotePort { get; set; }
}
