using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting;

public interface IStartup
{
    IServiceProvider ConfigureServices(IServiceCollection services);

    void Configure(IApplicationBuilder app);
}


public class EmptyStartup : StartupBase
{
    public override void Configure(IApplicationBuilder app)
    {
    }
}