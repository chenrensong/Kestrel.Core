using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Features;

public class TlsConnectionFeature : ITlsConnectionFeature
{
	public X509Certificate2 ClientCertificate { get; set; }

	public Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(ClientCertificate);
	}
}
