using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.WebUtilities;

public class BufferedReadStream : Stream
{
	private const byte CR = 13;

	private const byte LF = 10;

	private readonly Stream _inner;

	private readonly byte[] _buffer;

	private readonly ArrayPool<byte> _bytePool;

	private int _bufferOffset;

	private int _bufferCount;

	private bool _disposed;

	public ArraySegment<byte> BufferedData => new ArraySegment<byte>(_buffer, _bufferOffset, _bufferCount);

	public override bool CanRead
	{
		get
		{
			if (!_inner.CanRead)
			{
				return _bufferCount > 0;
			}
			return true;
		}
	}

	public override bool CanSeek => _inner.CanSeek;

	public override bool CanTimeout => _inner.CanTimeout;

	public override bool CanWrite => _inner.CanWrite;

	public override long Length => _inner.Length;

	public override long Position
	{
		get
		{
			return _inner.Position - _bufferCount;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", value, "Position must be positive.");
			}
			if (value == Position)
			{
				return;
			}
			if (value <= _inner.Position)
			{
				int num = (int)(_inner.Position - value);
				if (num <= _bufferCount)
				{
					_bufferOffset += num;
					_bufferCount -= num;
				}
				else
				{
					_bufferOffset = 0;
					_bufferCount = 0;
					_inner.Position = value;
				}
			}
			else
			{
				_bufferOffset = 0;
				_bufferCount = 0;
				_inner.Position = value;
			}
		}
	}

	public BufferedReadStream(Stream inner, int bufferSize)
		: this(inner, bufferSize, ArrayPool<byte>.Shared)
	{
	}

	public BufferedReadStream(Stream inner, int bufferSize, ArrayPool<byte> bytePool)
	{
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		_inner = inner;
		_bytePool = bytePool;
		_buffer = bytePool.Rent(bufferSize);
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
		_inner.SetLength(value);
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_disposed = true;
			_bytePool.Return(_buffer);
			if (disposing)
			{
				_inner.Dispose();
			}
		}
	}

	public override void Flush()
	{
		_inner.Flush();
	}

	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		return _inner.FlushAsync(cancellationToken);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		_inner.Write(buffer, offset, count);
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		return _inner.WriteAsync(buffer, offset, count, cancellationToken);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		ValidateBuffer(buffer, offset, count);
		if (_bufferCount > 0)
		{
			int num = Math.Min(_bufferCount, count);
			Buffer.BlockCopy(_buffer, _bufferOffset, buffer, offset, num);
			_bufferOffset += num;
			_bufferCount -= num;
			return num;
		}
		return _inner.Read(buffer, offset, count);
	}

	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ValidateBuffer(buffer, offset, count);
		if (_bufferCount > 0)
		{
			int num = Math.Min(_bufferCount, count);
			Buffer.BlockCopy(_buffer, _bufferOffset, buffer, offset, num);
			_bufferOffset += num;
			_bufferCount -= num;
			return num;
		}
		return await _inner.ReadAsync(buffer, offset, count, cancellationToken);
	}

	public bool EnsureBuffered()
	{
		if (_bufferCount > 0)
		{
			return true;
		}
		_bufferOffset = 0;
		_bufferCount = _inner.Read(_buffer, 0, _buffer.Length);
		return _bufferCount > 0;
	}

	public async Task<bool> EnsureBufferedAsync(CancellationToken cancellationToken)
	{
		if (_bufferCount > 0)
		{
			return true;
		}
		_bufferOffset = 0;
		_bufferCount = await _inner.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken);
		return _bufferCount > 0;
	}

	public bool EnsureBuffered(int minCount)
	{
		if (minCount > _buffer.Length)
		{
			throw new ArgumentOutOfRangeException("minCount", minCount, "The value must be smaller than the buffer size: " + _buffer.Length);
		}
		while (_bufferCount < minCount)
		{
			if (_bufferOffset > 0)
			{
				if (_bufferCount > 0)
				{
					Buffer.BlockCopy(_buffer, _bufferOffset, _buffer, 0, _bufferCount);
				}
				_bufferOffset = 0;
			}
			int num = _inner.Read(_buffer, _bufferOffset + _bufferCount, _buffer.Length - _bufferCount - _bufferOffset);
			_bufferCount += num;
			if (num == 0)
			{
				return false;
			}
		}
		return true;
	}

	public async Task<bool> EnsureBufferedAsync(int minCount, CancellationToken cancellationToken)
	{
		if (minCount > _buffer.Length)
		{
			throw new ArgumentOutOfRangeException("minCount", minCount, "The value must be smaller than the buffer size: " + _buffer.Length);
		}
		while (_bufferCount < minCount)
		{
			if (_bufferOffset > 0)
			{
				if (_bufferCount > 0)
				{
					Buffer.BlockCopy(_buffer, _bufferOffset, _buffer, 0, _bufferCount);
				}
				_bufferOffset = 0;
			}
			int num = await _inner.ReadAsync(_buffer, _bufferOffset + _bufferCount, _buffer.Length - _bufferCount - _bufferOffset, cancellationToken);
			_bufferCount += num;
			if (num == 0)
			{
				return false;
			}
		}
		return true;
	}

	public string ReadLine(int lengthLimit)
	{
		CheckDisposed();
		using MemoryStream memoryStream = new MemoryStream(200);
		bool foundCR = false;
		bool foundCRLF = false;
		while (!foundCRLF && EnsureBuffered())
		{
			if (memoryStream.Length > lengthLimit)
			{
				throw new InvalidDataException($"Line length limit {lengthLimit} exceeded.");
			}
			ProcessLineChar(memoryStream, ref foundCR, ref foundCRLF);
		}
		return DecodeLine(memoryStream, foundCRLF);
	}

	public async Task<string> ReadLineAsync(int lengthLimit, CancellationToken cancellationToken)
	{
		CheckDisposed();
		using MemoryStream builder = new MemoryStream(200);
		bool foundCR = false;
		bool foundCRLF = false;
		while (true)
		{
			bool flag = !foundCRLF;
			if (flag)
			{
				flag = await EnsureBufferedAsync(cancellationToken);
			}
			if (!flag)
			{
				break;
			}
			if (builder.Length > lengthLimit)
			{
				throw new InvalidDataException($"Line length limit {lengthLimit} exceeded.");
			}
			ProcessLineChar(builder, ref foundCR, ref foundCRLF);
		}
		return DecodeLine(builder, foundCRLF);
	}

	private void ProcessLineChar(MemoryStream builder, ref bool foundCR, ref bool foundCRLF)
	{
		byte b = _buffer[_bufferOffset];
		builder.WriteByte(b);
		_bufferOffset++;
		_bufferCount--;
		if ((b == 10) & foundCR)
		{
			foundCRLF = true;
		}
		else
		{
			foundCR = b == 13;
		}
	}

	private string DecodeLine(MemoryStream builder, bool foundCRLF)
	{
		long num = (foundCRLF ? (builder.Length - 2) : builder.Length);
		return Encoding.UTF8.GetString(builder.ToArray(), 0, (int)num);
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("BufferedReadStream");
		}
	}

	private void ValidateBuffer(byte[] buffer, int offset, int count)
	{
		new ArraySegment<byte>(buffer, offset, count);
		if (count == 0)
		{
			throw new ArgumentOutOfRangeException("count", "The value must be greater than zero.");
		}
	}
}
