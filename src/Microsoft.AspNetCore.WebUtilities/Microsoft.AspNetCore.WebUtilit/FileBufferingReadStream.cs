using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.WebUtilities;

public class FileBufferingReadStream : Stream
{
	private const int _maxRentedBufferSize = 1048576;

	private readonly Stream _inner;

	private readonly ArrayPool<byte> _bytePool;

	private readonly int _memoryThreshold;

	private readonly long? _bufferLimit;

	private string _tempFileDirectory;

	private readonly Func<string> _tempFileDirectoryAccessor;

	private string _tempFileName;

	private Stream _buffer;

	private byte[] _rentedBuffer;

	private bool _inMemory = true;

	private bool _completelyBuffered;

	private bool _disposed;

	public bool InMemory => _inMemory;

	public string TempFileName => _tempFileName;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => _buffer.Length;

	public override long Position
	{
		get
		{
			return _buffer.Position;
		}
		set
		{
			ThrowIfDisposed();
			_buffer.Position = value;
		}
	}

	public FileBufferingReadStream(Stream inner, int memoryThreshold, long? bufferLimit, Func<string> tempFileDirectoryAccessor)
		: this(inner, memoryThreshold, bufferLimit, tempFileDirectoryAccessor, ArrayPool<byte>.Shared)
	{
	}

	public FileBufferingReadStream(Stream inner, int memoryThreshold, long? bufferLimit, Func<string> tempFileDirectoryAccessor, ArrayPool<byte> bytePool)
	{
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		if (tempFileDirectoryAccessor == null)
		{
			throw new ArgumentNullException("tempFileDirectoryAccessor");
		}
		_bytePool = bytePool;
		if (memoryThreshold < 1048576)
		{
			_rentedBuffer = bytePool.Rent(memoryThreshold);
			_buffer = new MemoryStream(_rentedBuffer);
			_buffer.SetLength(0L);
		}
		else
		{
			_buffer = new MemoryStream();
		}
		_inner = inner;
		_memoryThreshold = memoryThreshold;
		_bufferLimit = bufferLimit;
		_tempFileDirectoryAccessor = tempFileDirectoryAccessor;
	}

	public FileBufferingReadStream(Stream inner, int memoryThreshold, long? bufferLimit, string tempFileDirectory)
		: this(inner, memoryThreshold, bufferLimit, tempFileDirectory, ArrayPool<byte>.Shared)
	{
	}

	public FileBufferingReadStream(Stream inner, int memoryThreshold, long? bufferLimit, string tempFileDirectory, ArrayPool<byte> bytePool)
	{
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		if (tempFileDirectory == null)
		{
			throw new ArgumentNullException("tempFileDirectory");
		}
		_bytePool = bytePool;
		if (memoryThreshold < 1048576)
		{
			_rentedBuffer = bytePool.Rent(memoryThreshold);
			_buffer = new MemoryStream(_rentedBuffer);
			_buffer.SetLength(0L);
		}
		else
		{
			_buffer = new MemoryStream();
		}
		_inner = inner;
		_memoryThreshold = memoryThreshold;
		_bufferLimit = bufferLimit;
		_tempFileDirectory = tempFileDirectory;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		ThrowIfDisposed();
		if (!_completelyBuffered && origin == SeekOrigin.End)
		{
			throw new NotSupportedException("The content has not been fully buffered yet.");
		}
		if (!_completelyBuffered && origin == SeekOrigin.Current && offset + Position > Length)
		{
			throw new NotSupportedException("The content has not been fully buffered yet.");
		}
		if (!_completelyBuffered && origin == SeekOrigin.Begin && offset > Length)
		{
			throw new NotSupportedException("The content has not been fully buffered yet.");
		}
		return _buffer.Seek(offset, origin);
	}

	private Stream CreateTempFile()
	{
		if (_tempFileDirectory == null)
		{
			_tempFileDirectory = _tempFileDirectoryAccessor();
		}
		_tempFileName = Path.Combine(_tempFileDirectory, "ASPNETCORE_" + Guid.NewGuid().ToString() + ".tmp");
		return new FileStream(_tempFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Delete, 16384, FileOptions.Asynchronous | FileOptions.DeleteOnClose | FileOptions.SequentialScan);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		ThrowIfDisposed();
		if (_buffer.Position < _buffer.Length || _completelyBuffered)
		{
			return _buffer.Read(buffer, offset, (int)Math.Min(count, _buffer.Length - _buffer.Position));
		}
		int num = _inner.Read(buffer, offset, count);
		if (_bufferLimit.HasValue && _bufferLimit - num < _buffer.Length)
		{
			Dispose();
			throw new IOException("Buffer limit exceeded.");
		}
		if (_inMemory && _buffer.Length + num > _memoryThreshold)
		{
			_inMemory = false;
			Stream buffer2 = _buffer;
			_buffer = CreateTempFile();
			if (_rentedBuffer == null)
			{
				buffer2.Position = 0L;
				byte[] array = _bytePool.Rent(Math.Min((int)buffer2.Length, 1048576));
				for (int num2 = buffer2.Read(array, 0, array.Length); num2 > 0; num2 = buffer2.Read(array, 0, array.Length))
				{
					_buffer.Write(array, 0, num2);
				}
				_bytePool.Return(array);
			}
			else
			{
				_buffer.Write(_rentedBuffer, 0, (int)buffer2.Length);
				_bytePool.Return(_rentedBuffer);
				_rentedBuffer = null;
			}
		}
		if (num > 0)
		{
			_buffer.Write(buffer, offset, num);
		}
		else
		{
			_completelyBuffered = true;
		}
		return num;
	}

	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ThrowIfDisposed();
		if (_buffer.Position < _buffer.Length || _completelyBuffered)
		{
			return await _buffer.ReadAsync(buffer, offset, (int)Math.Min(count, _buffer.Length - _buffer.Position), cancellationToken);
		}
		int read = await _inner.ReadAsync(buffer, offset, count, cancellationToken);
		if (_bufferLimit.HasValue && _bufferLimit - read < _buffer.Length)
		{
			Dispose();
			throw new IOException("Buffer limit exceeded.");
		}
		if (_inMemory && _buffer.Length + read > _memoryThreshold)
		{
			_inMemory = false;
			Stream oldBuffer = _buffer;
			_buffer = CreateTempFile();
			if (_rentedBuffer == null)
			{
				oldBuffer.Position = 0L;
				byte[] rentedBuffer = _bytePool.Rent(Math.Min((int)oldBuffer.Length, 1048576));
				for (int num = oldBuffer.Read(rentedBuffer, 0, rentedBuffer.Length); num > 0; num = oldBuffer.Read(rentedBuffer, 0, rentedBuffer.Length))
				{
					await _buffer.WriteAsync(rentedBuffer, 0, num, cancellationToken);
				}
				_bytePool.Return(rentedBuffer);
			}
			else
			{
				await _buffer.WriteAsync(_rentedBuffer, 0, (int)oldBuffer.Length, cancellationToken);
				_bytePool.Return(_rentedBuffer);
				_rentedBuffer = null;
			}
		}
		if (read > 0)
		{
			await _buffer.WriteAsync(buffer, offset, read, cancellationToken);
		}
		else
		{
			_completelyBuffered = true;
		}
		return read;
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
		if (!_disposed)
		{
			_disposed = true;
			if (_rentedBuffer != null)
			{
				_bytePool.Return(_rentedBuffer);
			}
			if (disposing)
			{
				_buffer.Dispose();
			}
		}
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("FileBufferingReadStream");
		}
	}
}
