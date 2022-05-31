using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http.Internal;

public struct HeaderSegmentCollection : IEnumerable<HeaderSegment>, IEnumerable, IEquatable<HeaderSegmentCollection>
{
	public struct Enumerator : IEnumerator<HeaderSegment>, IEnumerator, IDisposable
	{
		private enum Mode
		{
			Leading,
			Value,
			ValueQuoted,
			Trailing,
			Produce
		}

		private enum Attr
		{
			Value,
			Quote,
			Delimiter,
			Whitespace
		}

		private readonly StringValues _headers;

		private int _index;

		private string _header;

		private int _headerLength;

		private int _offset;

		private int _leadingStart;

		private int _leadingEnd;

		private int _valueStart;

		private int _valueEnd;

		private int _trailingStart;

		private Mode _mode;

		public HeaderSegment Current => new HeaderSegment(new StringSegment(_header, _leadingStart, _leadingEnd - _leadingStart), new StringSegment(_header, _valueStart, _valueEnd - _valueStart));

		object IEnumerator.Current => Current;

		public Enumerator(StringValues headers)
		{
			_headers = headers;
			_header = string.Empty;
			_headerLength = -1;
			_index = -1;
			_offset = -1;
			_leadingStart = -1;
			_leadingEnd = -1;
			_valueStart = -1;
			_valueEnd = -1;
			_trailingStart = -1;
			_mode = Mode.Leading;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_mode == Mode.Produce)
			{
				_leadingStart = _trailingStart;
				_leadingEnd = -1;
				_valueStart = -1;
				_valueEnd = -1;
				_trailingStart = -1;
				if (_offset == _headerLength && _leadingStart != -1 && _leadingStart != _offset)
				{
					_leadingEnd = _offset;
					return true;
				}
				_mode = Mode.Leading;
			}
			if (_offset == _headerLength)
			{
				_index++;
				_offset = -1;
				_leadingStart = 0;
				_leadingEnd = -1;
				_valueStart = -1;
				_valueEnd = -1;
				_trailingStart = -1;
				if (_index == _headers.Count)
				{
					return false;
				}
				_header = _headers[_index] ?? string.Empty;
				_headerLength = _header.Length;
			}
			do
			{
				_offset++;
				char c = ((_offset != _headerLength) ? _header[_offset] : '\0');
				int num;
				if (!char.IsWhiteSpace(c))
				{
					switch (c)
					{
					default:
						num = 0;
						break;
					case '\0':
					case ',':
						num = 2;
						break;
					case '"':
						num = 1;
						break;
					}
				}
				else
				{
					num = 3;
				}
				Attr attr = (Attr)num;
				switch (_mode)
				{
				case Mode.Leading:
					switch (attr)
					{
					case Attr.Delimiter:
						_valueStart = ((_valueStart == -1) ? _offset : _valueStart);
						_valueEnd = ((_valueEnd == -1) ? _offset : _valueEnd);
						_trailingStart = ((_trailingStart == -1) ? _offset : _trailingStart);
						_leadingEnd = _offset;
						_mode = Mode.Produce;
						break;
					case Attr.Quote:
						_leadingEnd = _offset;
						_valueStart = _offset;
						_mode = Mode.ValueQuoted;
						break;
					case Attr.Value:
						_leadingEnd = _offset;
						_valueStart = _offset;
						_mode = Mode.Value;
						break;
					}
					break;
				case Mode.Value:
					switch (attr)
					{
					case Attr.Quote:
						_mode = Mode.ValueQuoted;
						break;
					case Attr.Delimiter:
						_valueEnd = _offset;
						_trailingStart = _offset;
						_mode = Mode.Produce;
						break;
					case Attr.Whitespace:
						_valueEnd = _offset;
						_trailingStart = _offset;
						_mode = Mode.Trailing;
						break;
					}
					break;
				case Mode.ValueQuoted:
					switch (attr)
					{
					case Attr.Quote:
						_mode = Mode.Value;
						break;
					case Attr.Delimiter:
						if (c == '\0')
						{
							_valueEnd = _offset;
							_trailingStart = _offset;
							_mode = Mode.Produce;
						}
						break;
					}
					break;
				case Mode.Trailing:
					switch (attr)
					{
					case Attr.Delimiter:
						_mode = Mode.Produce;
						break;
					case Attr.Quote:
						_trailingStart = -1;
						_valueEnd = -1;
						_mode = Mode.ValueQuoted;
						break;
					case Attr.Value:
						_trailingStart = -1;
						_valueEnd = -1;
						_mode = Mode.Value;
						break;
					}
					break;
				}
			}
			while (_mode != Mode.Produce);
			return true;
		}

		public void Reset()
		{
			_index = 0;
			_offset = 0;
			_leadingStart = 0;
			_leadingEnd = 0;
			_valueStart = 0;
			_valueEnd = 0;
		}
	}

	private readonly StringValues _headers;

	public HeaderSegmentCollection(StringValues headers)
	{
		_headers = headers;
	}

	public bool Equals(HeaderSegmentCollection other)
	{
		return StringValues.Equals(_headers, other._headers);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is HeaderSegmentCollection)
		{
			return Equals((HeaderSegmentCollection)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (StringValues.IsNullOrEmpty(_headers))
		{
			return 0;
		}
		return _headers.GetHashCode();
	}

	public static bool operator ==(HeaderSegmentCollection left, HeaderSegmentCollection right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(HeaderSegmentCollection left, HeaderSegmentCollection right)
	{
		return !left.Equals(right);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_headers);
	}

	IEnumerator<HeaderSegment> IEnumerable<HeaderSegment>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
