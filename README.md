# Kestrel.Core
A cross platform web server with no external dependencies

This project is part of ASP.NET Core. support any .net 6.0 Application (include maui)

## How to work
```csharp
        var builder = new WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseKestrel(options =>
        {
        });
        var webHost = builder.Build();
        webHost.Run();
```