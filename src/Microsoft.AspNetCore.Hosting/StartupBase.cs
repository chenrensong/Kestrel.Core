using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting;

public abstract class StartupBase : IStartup
{
	public abstract void Configure(IApplicationBuilder app);

	IServiceProvider IStartup.ConfigureServices(IServiceCollection services)
	{
		ConfigureServices(services);
		return CreateServiceProvider(services);
	}

	public virtual void ConfigureServices(IServiceCollection services)
	{
	}

	public virtual IServiceProvider CreateServiceProvider(IServiceCollection services)
	{
		return services.BuildServiceProvider();
	}
}
public abstract class StartupBase<TBuilder> : StartupBase
{
	private readonly IServiceProviderFactory<TBuilder> _factory;

	public StartupBase(IServiceProviderFactory<TBuilder> factory)
	{
		_factory = factory;
	}

	public override IServiceProvider CreateServiceProvider(IServiceCollection services)
	{
		TBuilder val = _factory.CreateBuilder(services);
		ConfigureContainer(val);
		return _factory.CreateServiceProvider(val);
	}

	public virtual void ConfigureContainer(TBuilder builder)
	{
	}
}
