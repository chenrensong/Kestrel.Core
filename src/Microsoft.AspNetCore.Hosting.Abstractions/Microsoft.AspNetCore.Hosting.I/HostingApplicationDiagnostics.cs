using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Hosting.Internal;

internal class HostingApplicationDiagnostics
{
	private static readonly double TimestampToTicks = 10000000.0 / (double)Stopwatch.Frequency;

	private const string ActivityName = "Microsoft.AspNetCore.Hosting.HttpRequestIn";

	private const string ActivityStartKey = "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start";

	private const string DeprecatedDiagnosticsBeginRequestKey = "Microsoft.AspNetCore.Hosting.BeginRequest";

	private const string DeprecatedDiagnosticsEndRequestKey = "Microsoft.AspNetCore.Hosting.EndRequest";

	private const string DiagnosticsUnhandledExceptionKey = "Microsoft.AspNetCore.Hosting.UnhandledException";

	private const string RequestIdHeaderName = "Request-Id";

	private const string CorrelationContextHeaderName = "Correlation-Context";

	private readonly DiagnosticListener _diagnosticListener;

	private readonly ILogger _logger;

	public HostingApplicationDiagnostics(ILogger logger, DiagnosticListener diagnosticListener)
	{
		_logger = logger;
		_diagnosticListener = diagnosticListener;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void BeginRequest(HttpContext httpContext, ref HostingApplication.Context context)
	{
		long num = 0L;
		if (HostingEventSource.Log.IsEnabled())
		{
			context.EventLogEnabled = true;
			RecordRequestStartEventLog(httpContext);
		}
		bool num2 = _diagnosticListener.IsEnabled();
		bool flag = _logger.IsEnabled(LogLevel.Critical);
		StringValues value = default(StringValues);
		if (num2 || flag)
		{
			httpContext.Request.Headers.TryGetValue("Request-Id", out value);
		}
		if (num2)
		{
			if (_diagnosticListener.IsEnabled("Microsoft.AspNetCore.Hosting.HttpRequestIn", httpContext))
			{
				context.Activity = StartActivity(httpContext, value);
			}
			if (_diagnosticListener.IsEnabled("Microsoft.AspNetCore.Hosting.BeginRequest"))
			{
				num = Stopwatch.GetTimestamp();
				RecordBeginRequestDiagnostics(httpContext, num);
			}
		}
		if (flag)
		{
			context.Scope = _logger.RequestScope(httpContext, value);
			if (_logger.IsEnabled(LogLevel.Information))
			{
				if (num == 0L)
				{
					num = Stopwatch.GetTimestamp();
				}
				LogRequestStarting(httpContext);
			}
		}
		context.StartTimestamp = num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RequestEnd(HttpContext httpContext, Exception exception, HostingApplication.Context context)
	{
		long startTimestamp = context.StartTimestamp;
		long num = 0L;
		if (startTimestamp != 0L)
		{
			num = Stopwatch.GetTimestamp();
			LogRequestFinished(httpContext, startTimestamp, num);
		}
		if (_diagnosticListener.IsEnabled())
		{
			if (num == 0L)
			{
				num = Stopwatch.GetTimestamp();
			}
			if (exception == null)
			{
				if (_diagnosticListener.IsEnabled("Microsoft.AspNetCore.Hosting.EndRequest"))
				{
					RecordEndRequestDiagnostics(httpContext, num);
				}
			}
			else if (_diagnosticListener.IsEnabled("Microsoft.AspNetCore.Hosting.UnhandledException"))
			{
				RecordUnhandledExceptionDiagnostics(httpContext, num, exception);
			}
			Activity activity = context.Activity;
			if (activity != null)
			{
				StopActivity(httpContext, activity);
			}
		}
		if (context.EventLogEnabled && exception != null)
		{
			HostingEventSource.Log.UnhandledException();
		}
		context.Scope?.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ContextDisposed(HostingApplication.Context context)
	{
		if (context.EventLogEnabled)
		{
			HostingEventSource.Log.RequestStop();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void LogRequestStarting(HttpContext httpContext)
	{
		_logger.Log(LogLevel.Information, 1, new HostingRequestStartingLog(httpContext), null, HostingRequestStartingLog.Callback);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void LogRequestFinished(HttpContext httpContext, long startTimestamp, long currentTimestamp)
	{
		if (_logger.IsEnabled(LogLevel.Information))
		{
			TimeSpan elapsed = new TimeSpan((long)(TimestampToTicks * (double)(currentTimestamp - startTimestamp)));
			_logger.Log(LogLevel.Information, 2, new HostingRequestFinishedLog(httpContext, elapsed), null, HostingRequestFinishedLog.Callback);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RecordBeginRequestDiagnostics(HttpContext httpContext, long startTimestamp)
	{
		_diagnosticListener.Write("Microsoft.AspNetCore.Hosting.BeginRequest", new
		{
			httpContext = httpContext,
			timestamp = startTimestamp
		});
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RecordEndRequestDiagnostics(HttpContext httpContext, long currentTimestamp)
	{
		_diagnosticListener.Write("Microsoft.AspNetCore.Hosting.EndRequest", new
		{
			httpContext = httpContext,
			timestamp = currentTimestamp
		});
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void RecordUnhandledExceptionDiagnostics(HttpContext httpContext, long currentTimestamp, Exception exception)
	{
		_diagnosticListener.Write("Microsoft.AspNetCore.Hosting.UnhandledException", new
		{
			httpContext = httpContext,
			timestamp = currentTimestamp,
			exception = exception
		});
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void RecordRequestStartEventLog(HttpContext httpContext)
	{
		HostingEventSource.Log.RequestStart(httpContext.Request.Method, httpContext.Request.Path);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private Activity StartActivity(HttpContext httpContext, StringValues requestId)
	{
		Activity activity = new Activity("Microsoft.AspNetCore.Hosting.HttpRequestIn");
		if (!StringValues.IsNullOrEmpty(requestId))
		{
			activity.SetParentId(requestId);
			string[] commaSeparatedValues = httpContext.Request.Headers.GetCommaSeparatedValues("Correlation-Context");
			if (commaSeparatedValues != StringValues.Empty)
			{
				string[] array = commaSeparatedValues;
				for (int i = 0; i < array.Length; i++)
				{
					if (NameValueHeaderValue.TryParse(array[i], out var parsedValue))
					{
						activity.AddBaggage(parsedValue.Name.ToString(), parsedValue.Value.ToString());
					}
				}
			}
		}
		if (_diagnosticListener.IsEnabled("Microsoft.AspNetCore.Hosting.HttpRequestIn.Start"))
		{
			_diagnosticListener.StartActivity(activity, new
			{
				HttpContext = httpContext
			});
		}
		else
		{
			activity.Start();
		}
		return activity;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void StopActivity(HttpContext httpContext, Activity activity)
	{
		_diagnosticListener.StopActivity(activity, new
		{
			HttpContext = httpContext
		});
	}
}
