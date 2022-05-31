using System;
using System.Text;

namespace Microsoft.AspNetCore.WebUtilities;

internal class MultipartBoundary
{
	private readonly int[] _skipTable = new int[256];

	private readonly string _boundary;

	private bool _expectLeadingCrlf;

	public bool ExpectLeadingCrlf
	{
		get
		{
			return _expectLeadingCrlf;
		}
		set
		{
			if (value != _expectLeadingCrlf)
			{
				_expectLeadingCrlf = value;
				Initialize(_boundary, _expectLeadingCrlf);
			}
		}
	}

	public byte[] BoundaryBytes { get; private set; }

	public int FinalBoundaryLength { get; private set; }

	public MultipartBoundary(string boundary, bool expectLeadingCrlf = true)
	{
		if (boundary == null)
		{
			throw new ArgumentNullException("boundary");
		}
		_boundary = boundary;
		_expectLeadingCrlf = expectLeadingCrlf;
		Initialize(_boundary, _expectLeadingCrlf);
	}

	private void Initialize(string boundary, bool expectLeadingCrlf)
	{
		if (expectLeadingCrlf)
		{
			BoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary);
		}
		else
		{
			BoundaryBytes = Encoding.UTF8.GetBytes("--" + boundary);
		}
		FinalBoundaryLength = BoundaryBytes.Length + 2;
		int num = BoundaryBytes.Length;
		for (int i = 0; i < _skipTable.Length; i++)
		{
			_skipTable[i] = num;
		}
		for (int j = 0; j < num; j++)
		{
			_skipTable[BoundaryBytes[j]] = Math.Max(1, num - 1 - j);
		}
	}

	public int GetSkipValue(byte input)
	{
		return _skipTable[input];
	}
}
