using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class HostedServiceExecutor
{
	private readonly IEnumerable<IHostedService> _services;

	private readonly ILogger<HostedServiceExecutor> _logger;

	public HostedServiceExecutor(ILogger<HostedServiceExecutor> logger, IEnumerable<IHostedService> services)
	{
		_logger = logger;
		_services = services;
	}

	public async Task StartAsync(CancellationToken token)
	{
		try
		{
			await ExecuteAsync((IHostedService service) => service.StartAsync(token));
		}
		catch (Exception exception)
		{
			_logger.ApplicationError(9, "An error occurred starting the application", exception);
		}
	}

	public async Task StopAsync(CancellationToken token)
	{
		try
		{
			await ExecuteAsync((IHostedService service) => service.StopAsync(token));
		}
		catch (Exception exception)
		{
			_logger.ApplicationError(10, "An error occurred stopping the application", exception);
		}
	}

	private async Task ExecuteAsync(Func<IHostedService, Task> callback)
	{
		List<Exception> exceptions = null;
		foreach (IHostedService service in _services)
		{
			try
			{
				await callback(service);
			}
			catch (Exception item)
			{
				if (exceptions == null)
				{
					exceptions = new List<Exception>();
				}
				exceptions.Add(item);
			}
		}
		if (exceptions != null)
		{
			throw new AggregateException(exceptions);
		}
	}
}
