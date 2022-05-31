# Kestrel.Core
A cross platform web server

This project is part of ASP.NET Core. support any .net 6.0 Application (include MAUI)

## How to work
```csharp
var builder = new WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseKestrel(options =>{ });
var webHost = builder.Build();
webHost.Run();
```
