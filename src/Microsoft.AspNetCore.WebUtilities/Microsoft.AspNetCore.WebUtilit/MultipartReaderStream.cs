using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.WebUtilities;

internal class MultipartReaderStream : Stream
{
	private readonly MultipartBoundary _boundary;

	private readonly BufferedReadStream _innerStream;

	private readonly ArrayPool<byte> _bytePool;

	private readonly long _innerOffset;

	private long _position;

	private long _observedLength;

	private bool _finished;

	public bool FinalBoundaryFound { get; private set; }

	public long? LengthLimit { get; set; }

	public override bool CanRead => true;

	public override bool CanSeek => _innerStream.CanSeek;

	public override bool CanWrite => false;

	public override long Length => _observedLength;

	public override long Position
	{
		get
		{
			return _position;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", value, "The Position must be positive.");
			}
			if (value > _observedLength)
			{
				throw new ArgumentOutOfRangeException("value", value, "The Position must be less than length.");
			}
			_position = value;
			if (_position < _observedLength)
			{
				_finished = false;
			}
		}
	}

	public MultipartReaderStream(BufferedReadStream stream, MultipartBoundary boundary)
		: this(stream, boundary, ArrayPool<byte>.Shared)
	{
	}

	public MultipartReaderStream(BufferedReadStream stream, MultipartBoundary boundary, ArrayPool<byte> bytePool)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (boundary == null)
		{
			throw new ArgumentNullException("boundary");
		}
		_bytePool = bytePool;
		_innerStream = stream;
		_innerOffset = (_innerStream.CanSeek ? _innerStream.Position : 0);
		_boundary = boundary;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			Position = offset;
			break;
		case SeekOrigin.Current:
			Position += offset;
			break;
		default:
			Position = Length + offset;
			break;
		}
		return Position;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public override void Flush()
	{
		throw new NotSupportedException();
	}

	private void PositionInnerStream()
	{
		if (_innerStream.CanSeek && _innerStream.Position != _innerOffset + _position)
		{
			_innerStream.Position = _innerOffset + _position;
		}
	}

	private int UpdatePosition(int read)
	{
		_position += read;
		if (_observedLength < _position)
		{
			_observedLength = _position;
			if (LengthLimit.HasValue && _observedLength > LengthLimit.Value)
			{
				throw new InvalidDataException($"Multipart body length limit {LengthLimit.Value} exceeded.");
			}
		}
		return read;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_finished)
		{
			return 0;
		}
		PositionInnerStream();
		if (!_innerStream.EnsureBuffered(_boundary.FinalBoundaryLength))
		{
			throw new IOException("Unexpected end of Stream, the content may have already been read by another component. ");
		}
		ArraySegment<byte> bufferedData = _innerStream.BufferedData;
		int read;
		if (SubMatch(bufferedData, _boundary.BoundaryBytes, out var matchOffset, out var _))
		{
			if (matchOffset > bufferedData.Offset)
			{
				read = _innerStream.Read(buffer, offset, Math.Min(count, matchOffset - bufferedData.Offset));
				return UpdatePosition(read);
			}
			int num = _boundary.BoundaryBytes.Length;
			byte[] array = _bytePool.Rent(num);
			read = _innerStream.Read(array, 0, num);
			_bytePool.Return(array);
			string text = _innerStream.ReadLine(100);
			text = text.Trim();
			if (string.Equals("--", text, StringComparison.Ordinal))
			{
				FinalBoundaryFound = true;
			}
			_finished = true;
			return 0;
		}
		read = _innerStream.Read(buffer, offset, Math.Min(count, bufferedData.Count));
		return UpdatePosition(read);
	}

	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		if (_finished)
		{
			return 0;
		}
		PositionInnerStream();
		if (!(await _innerStream.EnsureBufferedAsync(_boundary.FinalBoundaryLength, cancellationToken)))
		{
			throw new IOException("Unexpected end of Stream, the content may have already been read by another component. ");
		}
		ArraySegment<byte> bufferedData = _innerStream.BufferedData;
		int read;
		if (SubMatch(bufferedData, _boundary.BoundaryBytes, out var matchOffset, out var _))
		{
			if (matchOffset > bufferedData.Offset)
			{
				read = _innerStream.Read(buffer, offset, Math.Min(count, matchOffset - bufferedData.Offset));
				return UpdatePosition(read);
			}
			int num = _boundary.BoundaryBytes.Length;
			byte[] array = _bytePool.Rent(num);
			_innerStream.Read(array, 0, num);
			_bytePool.Return(array);
			string b = (await _innerStream.ReadLineAsync(100, cancellationToken)).Trim();
			if (string.Equals("--", b, StringComparison.Ordinal))
			{
				FinalBoundaryFound = true;
			}
			_finished = true;
			return 0;
		}
		read = _innerStream.Read(buffer, offset, Math.Min(count, bufferedData.Count));
		return UpdatePosition(read);
	}

	private bool SubMatch(ArraySegment<byte> segment1, byte[] matchBytes, out int matchOffset, out int matchCount)
	{
		matchCount = 0;
		int num = matchBytes.Length - 1;
		byte b = matchBytes[num];
		int num2 = segment1.Offset + segment1.Count - matchBytes.Length;
		matchOffset = segment1.Offset;
		while (matchOffset < num2)
		{
			byte b2 = segment1.Array[matchOffset + num];
			if (b2 == b && CompareBuffers(segment1.Array, matchOffset, matchBytes, 0, num) == 0)
			{
				matchCount = matchBytes.Length;
				return true;
			}
			matchOffset += _boundary.GetSkipValue(b2);
		}
		int num3 = segment1.Offset + segment1.Count;
		matchCount = 0;
		while (matchOffset < num3)
		{
			int num4 = num3 - matchOffset;
			matchCount = 0;
			while (matchCount < matchBytes.Length && matchCount < num4)
			{
				if (matchBytes[matchCount] != segment1.Array[matchOffset + matchCount])
				{
					matchCount = 0;
					break;
				}
				matchCount++;
			}
			if (matchCount > 0)
			{
				break;
			}
			matchOffset++;
		}
		return matchCount > 0;
	}

	private static int CompareBuffers(byte[] buffer1, int offset1, byte[] buffer2, int offset2, int count)
	{
		while (count-- > 0)
		{
			if (buffer1[offset1] != buffer2[offset2])
			{
				return buffer1[offset1] - buffer2[offset2];
			}
			offset1++;
			offset2++;
		}
		return 0;
	}
}
