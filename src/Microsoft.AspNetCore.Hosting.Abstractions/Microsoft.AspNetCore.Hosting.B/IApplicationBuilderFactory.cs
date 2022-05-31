using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Hosting.Builder;

public interface IApplicationBuilderFactory
{
	IApplicationBuilder CreateBuilder(IFeatureCollection serverFeatures);
}
