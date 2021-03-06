using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting;

public class DelegateStartup : StartupBase<IServiceCollection>
{
	private Action<IApplicationBuilder> _configureApp;

	public DelegateStartup(IServiceProviderFactory<IServiceCollection> factory, Action<IApplicationBuilder> configureApp)
		: base(factory)
	{
		_configureApp = configureApp;
	}

	public override void Configure(IApplicationBuilder app)
	{
		_configureApp(app);
	}
}
