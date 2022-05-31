using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Hosting.Server;

public interface IServer : IDisposable
{
	IFeatureCollection Features { get; }

	Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken);

	Task StopAsync(CancellationToken cancellationToken);
}
