using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Hosting.Internal;


public class ApplicationLifetime : IApplicationLifetime, Microsoft.Extensions.Hosting.IApplicationLifetime
{
	private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();

	private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();

	private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();

	private readonly ILogger<ApplicationLifetime> _logger;

	public CancellationToken ApplicationStarted => _startedSource.Token;

	public CancellationToken ApplicationStopping => _stoppingSource.Token;

	public CancellationToken ApplicationStopped => _stoppedSource.Token;

	public ApplicationLifetime(ILogger<ApplicationLifetime> logger)
	{
		_logger = logger;
	}

	public void StopApplication()
	{
		lock (_stoppingSource)
		{
			try
			{
				ExecuteHandlers(_stoppingSource);
			}
			catch (Exception exception)
			{
				_logger.ApplicationError(7, "An error occurred stopping the application", exception);
			}
		}
	}

	public void NotifyStarted()
	{
		try
		{
			ExecuteHandlers(_startedSource);
		}
		catch (Exception exception)
		{
			_logger.ApplicationError(6, "An error occurred starting the application", exception);
		}
	}

	public void NotifyStopped()
	{
		try
		{
			ExecuteHandlers(_stoppedSource);
		}
		catch (Exception exception)
		{
			_logger.ApplicationError(8, "An error occurred stopping the application", exception);
		}
	}

	private void ExecuteHandlers(CancellationTokenSource cancel)
	{
		if (!cancel.IsCancellationRequested)
		{
			cancel.Cancel(throwOnFirstException: false);
		}
	}
}
