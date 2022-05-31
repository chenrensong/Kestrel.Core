using System;
using System.IO;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.Http.Internal;

public static class BufferingHelper
{
	internal const int DefaultBufferThreshold = 30720;

	private static readonly Func<string> _getTempDirectory = () => TempDirectory;

	private static string _tempDirectory;

	public static string TempDirectory
	{
		get
		{
			if (_tempDirectory == null)
			{
				string text = Environment.GetEnvironmentVariable("ASPNETCORE_TEMP") ?? Path.GetTempPath();
				if (!Directory.Exists(text))
				{
					throw new DirectoryNotFoundException(text);
				}
				_tempDirectory = text;
			}
			return _tempDirectory;
		}
	}

	public static HttpRequest EnableRewind(this HttpRequest request, int bufferThreshold = 30720, long? bufferLimit = null)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		Stream body = request.Body;
		if (!body.CanSeek)
		{
			FileBufferingReadStream disposable = (FileBufferingReadStream)(request.Body = new FileBufferingReadStream(body, bufferThreshold, bufferLimit, _getTempDirectory));
			request.HttpContext.Response.RegisterForDispose(disposable);
		}
		return request;
	}

	public static MultipartSection EnableRewind(this MultipartSection section, Action<IDisposable> registerForDispose, int bufferThreshold = 30720, long? bufferLimit = null)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		if (registerForDispose == null)
		{
			throw new ArgumentNullException("registerForDispose");
		}
		Stream body = section.Body;
		if (!body.CanSeek)
		{
			FileBufferingReadStream obj = (FileBufferingReadStream)(section.Body = new FileBufferingReadStream(body, bufferThreshold, bufferLimit, _getTempDirectory));
			registerForDispose(obj);
		}
		return section;
	}
}
