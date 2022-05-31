using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Builder;

public interface IApplicationBuilder
{
    IServiceProvider ApplicationServices { get; set; }

    IFeatureCollection ServerFeatures { get; }

    IDictionary<string, object> Properties { get; }

    IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware);

    IApplicationBuilder New();

    RequestDelegate Build();
}
