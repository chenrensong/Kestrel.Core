using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Features;

public interface ITlsConnectionFeature
{
	X509Certificate2 ClientCertificate { get; set; }

	Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken);
}
