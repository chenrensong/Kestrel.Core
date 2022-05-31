using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.WebUtilities;

public class HttpRequestStreamReader : TextReader
{
	private const int DefaultBufferSize = 1024;

	private const int MinBufferSize = 128;

	private const int MaxSharedBuilderCapacity = 360;

	private Stream _stream;

	private readonly Encoding _encoding;

	private readonly Decoder _decoder;

	private readonly ArrayPool<byte> _bytePool;

	private readonly ArrayPool<char> _charPool;

	private readonly int _byteBufferSize;

	private byte[] _byteBuffer;

	private char[] _charBuffer;

	private int _charBufferIndex;

	private int _charsRead;

	private int _bytesRead;

	private bool _isBlocked;

	private bool _disposed;

	public HttpRequestStreamReader(Stream stream, Encoding encoding)
		: this(stream, encoding, 1024, ArrayPool<byte>.Shared, ArrayPool<char>.Shared)
	{
	}

	public HttpRequestStreamReader(Stream stream, Encoding encoding, int bufferSize)
		: this(stream, encoding, bufferSize, ArrayPool<byte>.Shared, ArrayPool<char>.Shared)
	{
	}

	public HttpRequestStreamReader(Stream stream, Encoding encoding, int bufferSize, ArrayPool<byte> bytePool, ArrayPool<char> charPool)
	{
		_stream = stream ?? throw new ArgumentNullException("stream");
		_encoding = encoding ?? throw new ArgumentNullException("encoding");
		_bytePool = bytePool ?? throw new ArgumentNullException("bytePool");
		_charPool = charPool ?? throw new ArgumentNullException("charPool");
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize");
		}
		if (!stream.CanRead)
		{
			throw new ArgumentException(Resources.HttpRequestStreamReader_StreamNotReadable, "stream");
		}
		_byteBufferSize = bufferSize;
		_decoder = encoding.GetDecoder();
		_byteBuffer = _bytePool.Rent(bufferSize);
		try
		{
			int maxCharCount = encoding.GetMaxCharCount(bufferSize);
			_charBuffer = _charPool.Rent(maxCharCount);
		}
		catch
		{
			_bytePool.Return(_byteBuffer);
			if (_charBuffer != null)
			{
				_charPool.Return(_charBuffer);
			}
			throw;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !_disposed)
		{
			_disposed = true;
			_bytePool.Return(_byteBuffer);
			_charPool.Return(_charBuffer);
		}
		base.Dispose(disposing);
	}

	public override int Peek()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("HttpRequestStreamReader");
		}
		if (_charBufferIndex == _charsRead && (_isBlocked || ReadIntoBuffer() == 0))
		{
			return -1;
		}
		return _charBuffer[_charBufferIndex];
	}

	public override int Read()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("HttpRequestStreamReader");
		}
		if (_charBufferIndex == _charsRead && ReadIntoBuffer() == 0)
		{
			return -1;
		}
		return _charBuffer[_charBufferIndex++];
	}

	public override int Read(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0 || index + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (_disposed)
		{
			throw new ObjectDisposedException("HttpRequestStreamReader");
		}
		int num = 0;
		while (count > 0)
		{
			int num2 = _charsRead - _charBufferIndex;
			if (num2 == 0)
			{
				num2 = ReadIntoBuffer();
			}
			if (num2 == 0)
			{
				break;
			}
			if (num2 > count)
			{
				num2 = count;
			}
			Buffer.BlockCopy(_charBuffer, _charBufferIndex * 2, buffer, (index + num) * 2, num2 * 2);
			_charBufferIndex += num2;
			num += num2;
			count -= num2;
			if (_isBlocked)
			{
				break;
			}
		}
		return num;
	}

	public override async Task<int> ReadAsync(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0 || index + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (_disposed)
		{
			throw new ObjectDisposedException("HttpRequestStreamReader");
		}
		bool flag = _charBufferIndex == _charsRead;
		if (flag)
		{
			flag = await ReadIntoBufferAsync() == 0;
		}
		if (flag)
		{
			return 0;
		}
		int charsRead = 0;
		while (count > 0)
		{
			int i = _charsRead - _charBufferIndex;
			if (i == 0)
			{
				_charsRead = 0;
				_charBufferIndex = 0;
				_bytesRead = 0;
				do
				{
					_bytesRead = await _stream.ReadAsync(_byteBuffer, 0, _byteBufferSize);
					if (_bytesRead == 0)
					{
						_isBlocked = true;
						break;
					}
					_isBlocked = _bytesRead < _byteBufferSize;
					_charBufferIndex = 0;
					i = _decoder.GetChars(_byteBuffer, 0, _bytesRead, _charBuffer, 0);
					_charsRead += i;
				}
				while (i == 0);
				if (i == 0)
				{
					break;
				}
			}
			if (i > count)
			{
				i = count;
			}
			Buffer.BlockCopy(_charBuffer, _charBufferIndex * 2, buffer, (index + charsRead) * 2, i * 2);
			_charBufferIndex += i;
			charsRead += i;
			count -= i;
			if (_isBlocked)
			{
				break;
			}
		}
		return charsRead;
	}

	private int ReadIntoBuffer()
	{
		_charsRead = 0;
		_charBufferIndex = 0;
		_bytesRead = 0;
		do
		{
			_bytesRead = _stream.Read(_byteBuffer, 0, _byteBufferSize);
			if (_bytesRead == 0)
			{
				return _charsRead;
			}
			_isBlocked = _bytesRead < _byteBufferSize;
			_charsRead += _decoder.GetChars(_byteBuffer, 0, _bytesRead, _charBuffer, _charsRead);
		}
		while (_charsRead == 0);
		return _charsRead;
	}

	private async Task<int> ReadIntoBufferAsync()
	{
		_charsRead = 0;
		_charBufferIndex = 0;
		_bytesRead = 0;
		do
		{
			_bytesRead = await _stream.ReadAsync(_byteBuffer, 0, _byteBufferSize).ConfigureAwait(continueOnCapturedContext: false);
			if (_bytesRead == 0)
			{
				return _charsRead;
			}
			_isBlocked = _bytesRead < _byteBufferSize;
			_charsRead += _decoder.GetChars(_byteBuffer, 0, _bytesRead, _charBuffer, _charsRead);
		}
		while (_charsRead == 0);
		return _charsRead;
	}
}
