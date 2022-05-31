using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebUtilities;

public class MultipartReader
{
	public const int DefaultHeadersCountLimit = 16;

	public const int DefaultHeadersLengthLimit = 16384;

	private const int DefaultBufferSize = 4096;

	private readonly BufferedReadStream _stream;

	private readonly MultipartBoundary _boundary;

	private MultipartReaderStream _currentStream;

	public int HeadersCountLimit { get; set; } = 16;


	public int HeadersLengthLimit { get; set; } = 16384;


	public long? BodyLengthLimit { get; set; }

	public MultipartReader(string boundary, Stream stream)
		: this(boundary, stream, 4096)
	{
	}

	public MultipartReader(string boundary, Stream stream, int bufferSize)
	{
		if (boundary == null)
		{
			throw new ArgumentNullException("boundary");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (bufferSize < boundary.Length + 8)
		{
			throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Insufficient buffer space, the buffer must be larger than the boundary: " + boundary);
		}
		_stream = new BufferedReadStream(stream, bufferSize);
		_boundary = new MultipartBoundary(boundary, expectLeadingCrlf: false);
		_currentStream = new MultipartReaderStream(_stream, _boundary)
		{
			LengthLimit = HeadersLengthLimit
		};
	}

	public async Task<MultipartSection> ReadNextSectionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		await _currentStream.DrainAsync(cancellationToken);
		if (_currentStream.FinalBoundaryFound)
		{
			await _stream.DrainAsync(HeadersLengthLimit, cancellationToken);
			return null;
		}
		Dictionary<string, StringValues> headers = await ReadHeadersAsync(cancellationToken);
		_boundary.ExpectLeadingCrlf = true;
		_currentStream = new MultipartReaderStream(_stream, _boundary)
		{
			LengthLimit = BodyLengthLimit
		};
		long? baseStreamOffset = (_stream.CanSeek ? new long?(_stream.Position) : null);
		return new MultipartSection
		{
			Headers = headers,
			Body = _currentStream,
			BaseStreamOffset = baseStreamOffset
		};
	}

	private async Task<Dictionary<string, StringValues>> ReadHeadersAsync(CancellationToken cancellationToken)
	{
		int totalSize = 0;
		KeyValueAccumulator accumulator = default(KeyValueAccumulator);
		string text = await _stream.ReadLineAsync(HeadersLengthLimit - totalSize, cancellationToken);
		while (!string.IsNullOrEmpty(text))
		{
			if (HeadersLengthLimit - totalSize < text.Length)
			{
				throw new InvalidDataException($"Multipart headers length limit {HeadersLengthLimit} exceeded.");
			}
			totalSize += text.Length;
			int num = text.IndexOf(':');
			if (num <= 0)
			{
				throw new InvalidDataException("Invalid header line: " + text);
			}
			string key = text.Substring(0, num);
			string value = text.Substring(num + 1, text.Length - num - 1).Trim();
			accumulator.Append(key, value);
			if (accumulator.KeyCount > HeadersCountLimit)
			{
				throw new InvalidDataException($"Multipart headers count limit {HeadersCountLimit} exceeded.");
			}
			text = await _stream.ReadLineAsync(HeadersLengthLimit - totalSize, cancellationToken);
		}
		return accumulator.GetResults();
	}
}
