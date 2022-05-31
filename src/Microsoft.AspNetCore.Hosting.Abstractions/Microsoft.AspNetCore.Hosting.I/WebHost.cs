using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.StackTrace.Sources;

namespace Microsoft.AspNetCore.Hosting.Internal;

internal class WebHost : IWebHost, IDisposable
{
    private static readonly string DeprecatedServerUrlsKey = "server.urls";

    private readonly IServiceCollection _applicationServiceCollection;

    private IStartup _startup;

    private ApplicationLifetime _applicationLifetime;

    private HostedServiceExecutor _hostedServiceExecutor;

    private readonly IServiceProvider _hostingServiceProvider;

    private readonly WebHostOptions _options;

    private readonly IConfiguration _config;

    private readonly AggregateException _hostingStartupErrors;

    private IServiceProvider _applicationServices;

    private ExceptionDispatchInfo _applicationServicesException;

    private ILogger<WebHost> _logger;

    private bool _stopped;

    internal WebHostOptions Options => _options;

    private IServer Server { get; set; }

    public IServiceProvider Services => _applicationServices;

    public IFeatureCollection ServerFeatures
    {
        get
        {
            EnsureServer();
            return Server?.Features;
        }
    }

    public WebHost(IServiceCollection appServices, IServiceProvider hostingServiceProvider, WebHostOptions options, IConfiguration config, AggregateException hostingStartupErrors)
    {
        if (appServices == null)
        {
            throw new ArgumentNullException("appServices");
        }
        if (hostingServiceProvider == null)
        {
            throw new ArgumentNullException("hostingServiceProvider");
        }
        if (config == null)
        {
            throw new ArgumentNullException("config");
        }
        _config = config;
        _hostingStartupErrors = hostingStartupErrors;
        _options = options;
        _applicationServiceCollection = appServices;
        _hostingServiceProvider = hostingServiceProvider;
        ServiceCollectionServiceExtensions.AddSingleton<IApplicationLifetime, ApplicationLifetime>(_applicationServiceCollection);
        ServiceCollectionServiceExtensions.AddSingleton(_applicationServiceCollection, (IServiceProvider sp) => ServiceProviderServiceExtensions.GetRequiredService<IApplicationLifetime>(sp) as Microsoft.Extensions.Hosting.IApplicationLifetime);
        ServiceCollectionServiceExtensions.AddSingleton<HostedServiceExecutor>(_applicationServiceCollection);
    }

    public void Initialize()
    {
        try
        {
            EnsureApplicationServices();
        }
        catch (Exception source)
        {
            if (_applicationServices == null)
            {
                _applicationServices = _applicationServiceCollection.BuildServiceProvider();
            }
            if (!_options.CaptureStartupErrors)
            {
                throw;
            }
            _applicationServicesException = ExceptionDispatchInfo.Capture(source);
        }
    }

    public void Start()
    {
        StartAsync().GetAwaiter().GetResult();
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        HostingEventSource.Log.HostStart();
        _logger = ServiceProviderServiceExtensions.GetRequiredService<ILogger<WebHost>>(_applicationServices);
        _logger.Starting();
        RequestDelegate application = BuildApplication();
        _applicationLifetime = ServiceProviderServiceExtensions.GetRequiredService<IApplicationLifetime>(_applicationServices) as ApplicationLifetime;
        _hostedServiceExecutor = ServiceProviderServiceExtensions.GetRequiredService<HostedServiceExecutor>(_applicationServices);
        HostingApplication application2 = new HostingApplication(diagnosticSource: ServiceProviderServiceExtensions.GetRequiredService<DiagnosticListener>(_applicationServices), httpContextFactory: ServiceProviderServiceExtensions.GetRequiredService<IHttpContextFactory>(_applicationServices), application: application, logger: _logger);
        await Server.StartAsync(application2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        _applicationLifetime?.NotifyStarted();
        await _hostedServiceExecutor.StartAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        _logger.Started();
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            foreach (string finalHostingStartupAssembly in _options.GetFinalHostingStartupAssemblies())
            {
                _logger.LogDebug("Loaded hosting startup assembly {assemblyName}", finalHostingStartupAssembly);
            }
        }
        if (_hostingStartupErrors == null)
        {
            return;
        }
        foreach (Exception innerException in _hostingStartupErrors.InnerExceptions)
        {
            _logger.HostingStartupAssemblyError(innerException);
        }
    }

    private void EnsureApplicationServices()
    {
        if (_applicationServices == null)
        {
            EnsureStartup();
            _applicationServices = _startup.ConfigureServices(_applicationServiceCollection);
        }
    }

    private void EnsureStartup()
    {
        if (_startup == null)
        {
            _startup = ServiceProviderServiceExtensions.GetService<IStartup>(_hostingServiceProvider);
            if (_startup == null)
            {
                _startup = new EmptyStartup();
                ServiceCollectionServiceExtensions.AddSingleton(_applicationServiceCollection,
                    typeof(IStartup), _startup.GetType());
                //throw new InvalidOperationException("No startup configured. Please specify startup via WebHostBuilder.UseStartup, WebHostBuilder.Configure, injecting IStartup or specifying the startup assembly via StartupAssemblyKey in the web host configuration.");
            }
        }
    }

    private RequestDelegate BuildApplication()
    {
        try
        {
            _applicationServicesException?.Throw();
            EnsureServer();
            IApplicationBuilder applicationBuilder = ServiceProviderServiceExtensions.GetRequiredService<IApplicationBuilderFactory>(_applicationServices).CreateBuilder(Server.Features);
            applicationBuilder.ApplicationServices = _applicationServices;
            IEnumerable<IStartupFilter>? service = ServiceProviderServiceExtensions.GetService<IEnumerable<IStartupFilter>>(_applicationServices);
            Action<IApplicationBuilder> action = _startup.Configure;
            foreach (IStartupFilter item in service.Reverse())
            {
                action = item.Configure(action);
            }
            action(applicationBuilder);
            return applicationBuilder.Build();
        }
        catch (Exception ex)
        {
            if (!_options.SuppressStatusMessages)
            {
                Console.WriteLine("Application startup exception: " + ex.ToString());
            }
            ServiceProviderServiceExtensions.GetRequiredService<ILogger<WebHost>>(_applicationServices).ApplicationError(ex);
            if (!_options.CaptureStartupErrors)
            {
                throw;
            }
            EnsureServer();
            IHostingEnvironment requiredService = ServiceProviderServiceExtensions.GetRequiredService<IHostingEnvironment>(_applicationServices);
            bool num = requiredService.IsDevelopment() || _options.DetailedErrors;
            ErrorPageModel errorPageModel = new ErrorPageModel
            {
                RuntimeDisplayName = RuntimeInformation.FrameworkDescription
            };
            string clrVersion = new AssemblyName(typeof(DefaultValueAttribute).GetTypeInfo().Assembly.FullName).Version.ToString();
            errorPageModel.RuntimeArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
            Assembly assembly = typeof(ErrorPage).GetTypeInfo().Assembly;
            errorPageModel.CurrentAssemblyVesion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            errorPageModel.ClrVersion = clrVersion;
            errorPageModel.OperatingSystemDescription = RuntimeInformation.OSDescription;
            if (num)
            {
                ExceptionDetailsProvider exceptionDetailsProvider = new ExceptionDetailsProvider(requiredService.ContentRootFileProvider, 6);
                errorPageModel.ErrorDetails = exceptionDetailsProvider.GetDetails(ex);
            }
            else
            {
                errorPageModel.ErrorDetails = new ExceptionDetails[0];
            }
            ErrorPage errorPage = new ErrorPage(errorPageModel);
            return delegate (HttpContext context)
            {
                context.Response.StatusCode = 500;
                context.Response.Headers["Cache-Control"] = "no-cache";
                return errorPage.ExecuteAsync(context);
            };
        }
    }

    private void EnsureServer()
    {
        if (Server != null)
        {
            return;
        }
        Server = ServiceProviderServiceExtensions.GetRequiredService<IServer>(_applicationServices);
        IServerAddressesFeature serverAddressesFeature = Server.Features?.Get<IServerAddressesFeature>();
        ICollection<string> collection = serverAddressesFeature?.Addresses;
        if (collection == null || collection.IsReadOnly || collection.Count != 0)
        {
            return;
        }
        string text = _config[WebHostDefaults.ServerUrlsKey] ?? _config[DeprecatedServerUrlsKey];
        if (!string.IsNullOrEmpty(text))
        {
            serverAddressesFeature.PreferHostingUrls = WebHostUtilities.ParseBool(_config, WebHostDefaults.PreferHostingUrlsKey);
            string[] array = text.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in array)
            {
                collection.Add(item);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        if (!_stopped)
        {
            _stopped = true;
            _logger?.Shutdown();
            CancellationToken token = new CancellationTokenSource(Options.ShutdownTimeout).Token;
            cancellationToken = (cancellationToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token).Token : token);
            _applicationLifetime?.StopApplication();
            if (Server != null)
            {
                await Server.StopAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            if (_hostedServiceExecutor != null)
            {
                await _hostedServiceExecutor.StopAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            _applicationLifetime?.NotifyStopped();
            HostingEventSource.Log.HostStop();
        }
    }

    public void Dispose()
    {
        if (!_stopped)
        {
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger?.ServerShutdownException(ex);
            }
        }
        (_applicationServices as IDisposable)?.Dispose();
        (_hostingServiceProvider as IDisposable)?.Dispose();
    }
}
