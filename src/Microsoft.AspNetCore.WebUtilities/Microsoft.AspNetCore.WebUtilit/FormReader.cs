using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebUtilities;

public class FormReader : IDisposable
{
	public const int DefaultValueCountLimit = 1024;

	public const int DefaultKeyLengthLimit = 2048;

	public const int DefaultValueLengthLimit = 4194304;

	private const int _rentedCharPoolLength = 8192;

	private readonly TextReader _reader;

	private readonly char[] _buffer;

	private readonly ArrayPool<char> _charPool;

	private readonly StringBuilder _builder = new StringBuilder();

	private int _bufferOffset;

	private int _bufferCount;

	private string _currentKey;

	private string _currentValue;

	private bool _endOfStream;

	private bool _disposed;

	public int ValueCountLimit { get; set; } = 1024;


	public int KeyLengthLimit { get; set; } = 2048;


	public int ValueLengthLimit { get; set; } = 4194304;


	public FormReader(string data)
		: this(data, ArrayPool<char>.Shared)
	{
	}

	public FormReader(string data, ArrayPool<char> charPool)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_buffer = charPool.Rent(8192);
		_charPool = charPool;
		_reader = new StringReader(data);
	}

	public FormReader(Stream stream)
		: this(stream, Encoding.UTF8, ArrayPool<char>.Shared)
	{
	}

	public FormReader(Stream stream, Encoding encoding)
		: this(stream, encoding, ArrayPool<char>.Shared)
	{
	}

	public FormReader(Stream stream, Encoding encoding, ArrayPool<char> charPool)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		_buffer = charPool.Rent(8192);
		_charPool = charPool;
		_reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, 2048, leaveOpen: true);
	}

	public KeyValuePair<string, string>? ReadNextPair()
	{
		ReadNextPairImpl();
		if (ReadSucceeded())
		{
			return new KeyValuePair<string, string>(_currentKey, _currentValue);
		}
		return null;
	}

	private void ReadNextPairImpl()
	{
		StartReadNextPair();
		while (!_endOfStream)
		{
			if (_bufferCount == 0)
			{
				Buffer();
			}
			if (TryReadNextPair())
			{
				break;
			}
		}
	}

	public async Task<KeyValuePair<string, string>?> ReadNextPairAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		await ReadNextPairAsyncImpl(cancellationToken);
		if (ReadSucceeded())
		{
			return new KeyValuePair<string, string>(_currentKey, _currentValue);
		}
		return null;
	}

	private async Task ReadNextPairAsyncImpl(CancellationToken cancellationToken = default(CancellationToken))
	{
		StartReadNextPair();
		while (!_endOfStream)
		{
			if (_bufferCount == 0)
			{
				await BufferAsync(cancellationToken);
			}
			if (TryReadNextPair())
			{
				break;
			}
		}
	}

	private void StartReadNextPair()
	{
		_currentKey = null;
		_currentValue = null;
	}

	private bool TryReadNextPair()
	{
		if (_currentKey == null)
		{
			if (!TryReadWord('=', KeyLengthLimit, out _currentKey))
			{
				return false;
			}
			if (_bufferCount == 0)
			{
				return false;
			}
		}
		if (_currentValue == null && !TryReadWord('&', ValueLengthLimit, out _currentValue))
		{
			return false;
		}
		return true;
	}

	private bool TryReadWord(char separator, int limit, out string value)
	{
		do
		{
			if (ReadChar(separator, limit, out value))
			{
				return true;
			}
		}
		while (_bufferCount > 0);
		return false;
	}

	private bool ReadChar(char separator, int limit, out string word)
	{
		if (_bufferCount == 0)
		{
			word = BuildWord();
			return true;
		}
		char c = _buffer[_bufferOffset++];
		_bufferCount--;
		if (c == separator)
		{
			word = BuildWord();
			return true;
		}
		if (_builder.Length >= limit)
		{
			throw new InvalidDataException($"Form key or value length limit {limit} exceeded.");
		}
		_builder.Append(c);
		word = null;
		return false;
	}

	private string BuildWord()
	{
		_builder.Replace('+', ' ');
		string stringToUnescape = _builder.ToString();
		_builder.Clear();
		return Uri.UnescapeDataString(stringToUnescape);
	}

	private void Buffer()
	{
		_bufferOffset = 0;
		_bufferCount = _reader.Read(_buffer, 0, _buffer.Length);
		_endOfStream = _bufferCount == 0;
	}

	private async Task BufferAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_bufferOffset = 0;
		_bufferCount = await _reader.ReadAsync(_buffer, 0, _buffer.Length);
		_endOfStream = _bufferCount == 0;
	}

	public Dictionary<string, StringValues> ReadForm()
	{
		KeyValueAccumulator accumulator = default(KeyValueAccumulator);
		while (!_endOfStream)
		{
			ReadNextPairImpl();
			Append(ref accumulator);
		}
		return accumulator.GetResults();
	}

	public async Task<Dictionary<string, StringValues>> ReadFormAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		KeyValueAccumulator accumulator = default(KeyValueAccumulator);
		while (!_endOfStream)
		{
			await ReadNextPairAsyncImpl(cancellationToken);
			Append(ref accumulator);
		}
		return accumulator.GetResults();
	}

	private bool ReadSucceeded()
	{
		if (_currentKey != null)
		{
			return _currentValue != null;
		}
		return false;
	}

	private void Append(ref KeyValueAccumulator accumulator)
	{
		if (ReadSucceeded())
		{
			accumulator.Append(_currentKey, _currentValue);
			if (accumulator.ValueCount > ValueCountLimit)
			{
				throw new InvalidDataException($"Form value count limit {ValueCountLimit} exceeded.");
			}
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_charPool.Return(_buffer);
		}
	}
}
