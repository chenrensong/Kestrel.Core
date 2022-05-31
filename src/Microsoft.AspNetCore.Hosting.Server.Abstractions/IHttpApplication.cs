using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Hosting.Server;

public interface IHttpApplication<TContext>
{
	TContext CreateContext(IFeatureCollection contextFeatures);

	Task ProcessRequestAsync(TContext context);

	void DisposeContext(TContext context, Exception exception);
}
