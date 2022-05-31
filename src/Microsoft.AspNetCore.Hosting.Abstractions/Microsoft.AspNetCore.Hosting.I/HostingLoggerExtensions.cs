using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Hosting.Internal;

internal static class HostingLoggerExtensions
{
	private class HostingLogScope : IReadOnlyList<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IReadOnlyCollection<KeyValuePair<string, object>>
	{
		private readonly string _path;

		private readonly string _traceIdentifier;

		private readonly string _correlationId;

		private string _cachedToString;

		public int Count => 3;

		public KeyValuePair<string, object> this[int index] => index switch
		{
			0 => new KeyValuePair<string, object>("RequestId", _traceIdentifier), 
			1 => new KeyValuePair<string, object>("RequestPath", _path), 
			2 => new KeyValuePair<string, object>("CorrelationId", _correlationId), 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};

		public HostingLogScope(HttpContext httpContext, string correlationId)
		{
			_traceIdentifier = httpContext.TraceIdentifier;
			_path = httpContext.Request.Path.ToString();
			_correlationId = correlationId;
		}

		public override string ToString()
		{
			if (_cachedToString == null)
			{
				_cachedToString = string.Format(CultureInfo.InvariantCulture, "RequestId:{0} RequestPath:{1}", _traceIdentifier, _path);
			}
			return _cachedToString;
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			int i = 0;
			while (i < Count)
			{
				yield return this[i];
				int num = i + 1;
				i = num;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public static IDisposable RequestScope(this ILogger logger, HttpContext httpContext, string correlationId)
	{
		return logger.BeginScope(new HostingLogScope(httpContext, correlationId));
	}

	public static void ApplicationError(this ILogger logger, Exception exception)
	{
		logger.ApplicationError(6, "Application startup exception", exception);
	}

	public static void HostingStartupAssemblyError(this ILogger logger, Exception exception)
	{
		logger.ApplicationError(11, "Hosting startup assembly exception", exception);
	}

	public static void ApplicationError(this ILogger logger, EventId eventId, string message, Exception exception)
	{
		if (exception is ReflectionTypeLoadException ex)
		{
			Exception[] loaderExceptions = ex.LoaderExceptions;
			foreach (Exception ex2 in loaderExceptions)
			{
				message = message + Environment.NewLine + ex2.Message;
			}
		}
		string message2 = message;
		logger.LogCritical(eventId, exception, message2);
	}

	public static void Starting(this ILogger logger)
	{
		if (logger.IsEnabled(LogLevel.Debug))
		{
			logger.LogDebug(3, "Hosting starting");
		}
	}

	public static void Started(this ILogger logger)
	{
		if (logger.IsEnabled(LogLevel.Debug))
		{
			logger.LogDebug(4, "Hosting started");
		}
	}

	public static void Shutdown(this ILogger logger)
	{
		if (logger.IsEnabled(LogLevel.Debug))
		{
			logger.LogDebug(5, "Hosting shutdown");
		}
	}

	public static void ServerShutdownException(this ILogger logger, Exception ex)
	{
		if (logger.IsEnabled(LogLevel.Debug))
		{
			logger.LogDebug(12, ex, "Server shutdown exception");
		}
	}
}
