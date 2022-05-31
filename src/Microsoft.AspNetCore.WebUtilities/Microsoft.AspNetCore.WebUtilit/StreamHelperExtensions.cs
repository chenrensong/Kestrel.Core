using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.WebUtilities;

public static class StreamHelperExtensions
{
	private const int _maxReadBufferSize = 4096;

	public static Task DrainAsync(this Stream stream, CancellationToken cancellationToken)
	{
		return stream.DrainAsync(ArrayPool<byte>.Shared, null, cancellationToken);
	}

	public static Task DrainAsync(this Stream stream, long? limit, CancellationToken cancellationToken)
	{
		return stream.DrainAsync(ArrayPool<byte>.Shared, limit, cancellationToken);
	}

	public static async Task DrainAsync(this Stream stream, ArrayPool<byte> bytePool, long? limit, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		byte[] buffer = bytePool.Rent(4096);
		long total = 0L;
		try
		{
			for (int num = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken); num > 0; num = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken))
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (limit.HasValue && limit.Value - total < num)
				{
					throw new InvalidDataException($"The stream exceeded the data limit {limit.Value}.");
				}
				total += num;
			}
		}
		finally
		{
			bytePool.Return(buffer);
		}
	}
}
