using System.Net;

namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpConnectionFeature
{
	string ConnectionId { get; set; }

	IPAddress RemoteIpAddress { get; set; }

	IPAddress LocalIpAddress { get; set; }

	int RemotePort { get; set; }

	int LocalPort { get; set; }
}
