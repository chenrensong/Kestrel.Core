using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Internal;

internal class ReferenceReadStream : Stream
{
	private readonly Stream _inner;

	private readonly long _innerOffset;

	private readonly long _length;

	private long _position;

	private bool _disposed;

	public override bool CanRead => true;

	public override bool CanSeek => _inner.CanSeek;

	public override bool CanWrite => false;

	public override long Length => _length;

	public override long Position
	{
		get
		{
			return _position;
		}
		set
		{
			ThrowIfDisposed();
			if (value < 0 || value > Length)
			{
				throw new ArgumentOutOfRangeException("value", value, "The Position must be within the length of the Stream: " + Length);
			}
			VerifyPosition();
			_position = value;
			_inner.Position = _innerOffset + _position;
		}
	}

	public ReferenceReadStream(Stream inner, long offset, long length)
	{
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		_inner = inner;
		_innerOffset = offset;
		_length = length;
		_inner.Position = offset;
	}

	private void VerifyPosition()
	{
		if (_inner.Position != _innerOffset + _position)
		{
			throw new InvalidOperationException("The inner stream position has changed unexpectedly.");
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			Position = offset;
			break;
		case SeekOrigin.End:
			Position = Length + offset;
			break;
		default:
			Position += offset;
			break;
		}
		return Position;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		ThrowIfDisposed();
		VerifyPosition();
		long num = Math.Min(count, _length - _position);
		int num2 = _inner.Read(buffer, offset, (int)num);
		_position += num2;
		return num2;
	}

	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ThrowIfDisposed();
		VerifyPosition();
		long num = Math.Min(count, _length - _position);
		int num2 = await _inner.ReadAsync(buffer, offset, (int)num, cancellationToken);
		_position += num2;
		return num2;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Flush()
	{
		throw new NotSupportedException();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposed = true;
		}
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ReferenceReadStream");
		}
	}
}
