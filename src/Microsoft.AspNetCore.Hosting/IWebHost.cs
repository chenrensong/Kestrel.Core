using Microsoft.AspNetCore.Http.Features;
namespace Microsoft.AspNetCore.Hosting;

public interface IWebHost : IDisposable
{
	IFeatureCollection ServerFeatures { get; }

	IServiceProvider Services { get; }

	void Start();

	Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));

	Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));
}
