using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Extensions;

public static class StreamCopyOperation
{
	private const int DefaultBufferSize = 4096;

	public static Task CopyToAsync(Stream source, Stream destination, long? count, CancellationToken cancel)
	{
		return CopyToAsync(source, destination, count, 4096, cancel);
	}

	public static async Task CopyToAsync(Stream source, Stream destination, long? count, int bufferSize, CancellationToken cancel)
	{
		long? bytesRemaining = count;
		byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
		try
		{
			while (!bytesRemaining.HasValue || bytesRemaining.Value > 0)
			{
				cancel.ThrowIfCancellationRequested();
				int num = buffer.Length;
				if (bytesRemaining.HasValue)
				{
					num = (int)Math.Min(bytesRemaining.Value, num);
				}
				int num2 = await source.ReadAsync(buffer, 0, num, cancel);
				if (bytesRemaining.HasValue)
				{
					bytesRemaining -= num2;
				}
				if (num2 == 0)
				{
					break;
				}
				cancel.ThrowIfCancellationRequested();
				await destination.WriteAsync(buffer, 0, num2, cancel);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}
