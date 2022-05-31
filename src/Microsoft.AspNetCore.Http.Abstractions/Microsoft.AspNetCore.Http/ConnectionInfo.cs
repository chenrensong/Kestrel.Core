using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public abstract class ConnectionInfo
{
	public abstract string Id { get; set; }

	public abstract IPAddress RemoteIpAddress { get; set; }

	public abstract int RemotePort { get; set; }

	public abstract IPAddress LocalIpAddress { get; set; }

	public abstract int LocalPort { get; set; }

	public abstract X509Certificate2 ClientCertificate { get; set; }

	public abstract Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = default(CancellationToken));
}
