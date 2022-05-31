using System;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

public static class MapWhenExtensions
{

    public static IApplicationBuilder MapWhen(this IApplicationBuilder app, Func<HttpContext, bool> predicate, Action<IApplicationBuilder> configuration)
    {
        if (app == null)
        {
            throw new ArgumentNullException("app");
        }
        if (predicate == null)
        {
            throw new ArgumentNullException("predicate");
        }
        if (configuration == null)
        {
            throw new ArgumentNullException("configuration");
        }
        IApplicationBuilder applicationBuilder = app.New();
        configuration(applicationBuilder);
        RequestDelegate branch = applicationBuilder.Build();
        MapWhenOptions options = new MapWhenOptions
        {
            Predicate = predicate,
            Branch = branch
        };
        return app.Use((RequestDelegate next) => new MapWhenMiddleware(next, options).Invoke);
    }

}
