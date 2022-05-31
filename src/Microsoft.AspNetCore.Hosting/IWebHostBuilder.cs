using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting;

public interface IWebHostBuilder
{
	IWebHost Build();

	IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate);

	IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

	IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices);

	string GetSetting(string key);

	IWebHostBuilder UseSetting(string key, string value);
}
