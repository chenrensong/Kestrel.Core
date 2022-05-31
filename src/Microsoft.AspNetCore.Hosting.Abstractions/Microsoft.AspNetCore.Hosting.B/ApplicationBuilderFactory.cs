using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Hosting.Builder;

public class ApplicationBuilderFactory : IApplicationBuilderFactory
{
	private readonly IServiceProvider _serviceProvider;

	public ApplicationBuilderFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public IApplicationBuilder CreateBuilder(IFeatureCollection serverFeatures)
	{
		return new ApplicationBuilder(_serviceProvider, serverFeatures);
	}
}
