using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Hosting;

public static class WebHostExtensions
{
    public static Task StopAsync(this IWebHost host, TimeSpan timeout)
    {
        return host.StopAsync(new CancellationTokenSource(timeout).Token);
    }

    public static void WaitForShutdown(this IWebHost host, bool isConsole = false)
    {
        host.WaitForShutdownAsync(isConsole).GetAwaiter().GetResult();
    }

    public static async Task WaitForShutdownAsync(this IWebHost host, bool isConsole, CancellationToken token = default(CancellationToken))
    {
        ManualResetEventSlim done = new ManualResetEventSlim(initialState: false);
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        AttachCtrlcSigtermShutdown(isConsole, cts, done, string.Empty);
        try
        {
            await host.WaitForTokenShutdownAsync(cts.Token);
        }
        finally
        {
            done.Set();
        }
    }

    public static void Run(this IWebHost host, bool isConsole = false)
    {
        host.RunAsync(isConsole).GetAwaiter().GetResult();
    }

    public static async Task RunAsync(this IWebHost host, bool isConsole = false, CancellationToken token = default(CancellationToken))
    {
        if (token.CanBeCanceled)
        {
            await host.RunAsync(token, null);
            return;
        }
        ManualResetEventSlim done = new ManualResetEventSlim(initialState: false);
        using CancellationTokenSource cts = new CancellationTokenSource();
        string shutdownMessage = (ServiceProviderServiceExtensions.GetRequiredService<WebHostOptions>(host.Services).SuppressStatusMessages ? string.Empty : "Application is shutting down...");
        AttachCtrlcSigtermShutdown(isConsole, cts, done, shutdownMessage);
        try
        {
            await host.RunAsync(cts.Token, "Application started. Press Ctrl+C to shut down.");
        }
        finally
        {
            done.Set();
        }
    }

    private static async Task RunAsync(this IWebHost host, CancellationToken token, string shutdownMessage)
    {
        using (host)
        {
            await host.StartAsync(token);
            var logger = ServiceProviderServiceExtensions.GetService<ILogger<WebHost>>(host.Services);
            IHostEnvironment service = ServiceProviderServiceExtensions.GetService<IHostEnvironment>(host.Services);
            if (!ServiceProviderServiceExtensions.GetRequiredService<WebHostOptions>(host.Services).SuppressStatusMessages)
            {
                logger?.LogInformation("Hosting environment: " + service.EnvironmentName);
                logger?.LogInformation("Content root path: " + service.ContentRootPath);
                ICollection<string> collection = host.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
                if (collection != null)
                {
                    foreach (string item in collection)
                    {
                        logger?.LogInformation("Now listening on: " + item);
                    }
                }
                if (!string.IsNullOrEmpty(shutdownMessage))
                {
                    logger?.LogInformation(shutdownMessage);
                }
            }
            await host.WaitForTokenShutdownAsync(token);
        }
    }

    private static void AttachCtrlcSigtermShutdown(bool isConsole, CancellationTokenSource cts, ManualResetEventSlim resetEvent, string shutdownMessage)
    {
        AppDomain.CurrentDomain.ProcessExit += delegate
        {
            Shutdown();
        };
        if (isConsole)
        {
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs eventArgs)
            {
                Shutdown();
                eventArgs.Cancel = true;
            };
        }
        void Shutdown()
        {
            try
            {
                if (!cts.IsCancellationRequested)
                {
                    if (!string.IsNullOrEmpty(shutdownMessage))
                    {
                        Console.WriteLine(shutdownMessage);
                    }
                    cts.Cancel();
                }
            }
            catch (Exception)
            {
            }
            resetEvent.Wait();
        }
    }

    private static async Task WaitForTokenShutdownAsync(this IWebHost host, CancellationToken token)
    {
        IApplicationLifetime service = ServiceProviderServiceExtensions.GetService<IApplicationLifetime>(host.Services);
        token.Register(delegate (object state)
        {
            ((IApplicationLifetime)state).StopApplication();
        }, service);
        TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        service.ApplicationStopping.Register(delegate (object obj)
        {
            ((TaskCompletionSource<object>)obj).TrySetResult(null);
        }, taskCompletionSource);
        await taskCompletionSource.Task;
        await host.StopAsync();
    }
}
