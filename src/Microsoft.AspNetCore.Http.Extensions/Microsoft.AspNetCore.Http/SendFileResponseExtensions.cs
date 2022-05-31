using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Http;

public static class SendFileResponseExtensions
{
	public static Task SendFileAsync(this HttpResponse response, IFileInfo file, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		return SendFileAsyncCore(response, file, 0L, null, cancellationToken);
	}

	public static Task SendFileAsync(this HttpResponse response, IFileInfo file, long offset, long? count, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		return SendFileAsyncCore(response, file, offset, count, cancellationToken);
	}

	public static Task SendFileAsync(this HttpResponse response, string fileName, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		return SendFileAsyncCore(response, fileName, 0L, null, cancellationToken);
	}

	public static Task SendFileAsync(this HttpResponse response, string fileName, long offset, long? count, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		return SendFileAsyncCore(response, fileName, offset, count, cancellationToken);
	}

	private static async Task SendFileAsyncCore(HttpResponse response, IFileInfo file, long offset, long? count, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(file.PhysicalPath))
		{
			CheckRange(offset, count, file.Length);
			using Stream fileContent = file.CreateReadStream();
			if (offset > 0)
			{
				fileContent.Seek(offset, SeekOrigin.Begin);
			}
			await StreamCopyOperation.CopyToAsync(fileContent, response.Body, count, cancellationToken);
		}
		else
		{
			await response.SendFileAsync(file.PhysicalPath, offset, count, cancellationToken);
		}
	}

	private static Task SendFileAsyncCore(HttpResponse response, string fileName, long offset, long? count, CancellationToken cancellationToken = default(CancellationToken))
	{
		IHttpSendFileFeature httpSendFileFeature = response.HttpContext.Features.Get<IHttpSendFileFeature>();
		if (httpSendFileFeature == null)
		{
			return SendFileAsyncCore(response.Body, fileName, offset, count, cancellationToken);
		}
		return httpSendFileFeature.SendFileAsync(fileName, offset, count, cancellationToken);
	}

	private static async Task SendFileAsyncCore(Stream outputStream, string fileName, long offset, long? count, CancellationToken cancel = default(CancellationToken))
	{
		cancel.ThrowIfCancellationRequested();
		FileInfo fileInfo = new FileInfo(fileName);
		CheckRange(offset, count, fileInfo.Length);
		int bufferSize = 16384;
		FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
		using (fileStream)
		{
			if (offset > 0)
			{
				fileStream.Seek(offset, SeekOrigin.Begin);
			}
			await StreamCopyOperation.CopyToAsync(fileStream, outputStream, count, cancel);
		}
	}

	private static void CheckRange(long offset, long? count, long fileLength)
	{
		if (offset < 0 || offset > fileLength)
		{
			throw new ArgumentOutOfRangeException("offset", offset, string.Empty);
		}
		if (count.HasValue && (count.Value < 0 || count.Value > fileLength - offset))
		{
			throw new ArgumentOutOfRangeException("count", count, string.Empty);
		}
	}
}
