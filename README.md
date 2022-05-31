# Kestrel.Core
A cross platform web server

This project is part of ASP.NET Core. support any .net 6.0 Application (include MAUI)

[![NuGet Version](https://img.shields.io/nuget/v/kestrel.core.svg?style=flat)](https://www.nuget.org/packages?q=kestrel.core) 

## How to work
```csharp
var builder = new WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .Configure(app =>
                      {
                          app.MapWhen(context =>
                          {
                              return context.Request.Path.Value.StartsWith("/hi");
                          }, builder =>
                          {
                              builder.Run(async (context) =>
                              {
                                  await context.Response.WriteAsync("hello world!");
                              });
                          });
                      })
        .UseKestrel(options =>{ });
var webHost = builder.Build();
webHost.Run();
```
