using System;
using System.Globalization;
using Microsoft.Extensions.WebEncoders.Sources;

namespace Microsoft.AspNetCore.WebUtilities;

public static class WebEncoders
{
	private static readonly byte[] EmptyBytes = new byte[0];

	public static byte[] Base64UrlDecode(string input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		return Base64UrlDecode(input, 0, input.Length);
	}

	public static byte[] Base64UrlDecode(string input, int offset, int count)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		ValidateParameters(input.Length, "input", offset, count);
		if (count == 0)
		{
			return EmptyBytes;
		}
		char[] buffer = new char[GetArraySizeRequiredToDecode(count)];
		return Base64UrlDecode(input, offset, buffer, 0, count);
	}

	public static byte[] Base64UrlDecode(string input, int offset, char[] buffer, int bufferOffset, int count)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		ValidateParameters(input.Length, "input", offset, count);
		if (bufferOffset < 0)
		{
			throw new ArgumentOutOfRangeException("bufferOffset");
		}
		if (count == 0)
		{
			return EmptyBytes;
		}
		int num = GetNumBase64PaddingCharsToAddForDecode(count);
		int num2 = checked(count + num);
		if (buffer.Length - bufferOffset < num2)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EncoderResources.WebEncoders_InvalidCountOffsetOrLength, "count", "bufferOffset", "input"), "count");
		}
		int num3 = bufferOffset;
		int num4 = offset;
		while (num3 - bufferOffset < count)
		{
			char c = input[num4];
			switch (c)
			{
			case '-':
				buffer[num3] = '+';
				break;
			case '_':
				buffer[num3] = '/';
				break;
			default:
				buffer[num3] = c;
				break;
			}
			num3++;
			num4++;
		}
		while (num > 0)
		{
			buffer[num3] = '=';
			num3++;
			num--;
		}
		return Convert.FromBase64CharArray(buffer, bufferOffset, num2);
	}

	public static int GetArraySizeRequiredToDecode(int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count == 0)
		{
			return 0;
		}
		int numBase64PaddingCharsToAddForDecode = GetNumBase64PaddingCharsToAddForDecode(count);
		return checked(count + numBase64PaddingCharsToAddForDecode);
	}

	public static string Base64UrlEncode(byte[] input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		return Base64UrlEncode(input, 0, input.Length);
	}

	public static string Base64UrlEncode(byte[] input, int offset, int count)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		ValidateParameters(input.Length, "input", offset, count);
		if (count == 0)
		{
			return string.Empty;
		}
		char[] array = new char[GetArraySizeRequiredToEncode(count)];
		int length = Base64UrlEncode(input, offset, array, 0, count);
		return new string(array, 0, length);
	}

	public static int Base64UrlEncode(byte[] input, int offset, char[] output, int outputOffset, int count)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		ValidateParameters(input.Length, "input", offset, count);
		if (outputOffset < 0)
		{
			throw new ArgumentOutOfRangeException("outputOffset");
		}
		int arraySizeRequiredToEncode = GetArraySizeRequiredToEncode(count);
		if (output.Length - outputOffset < arraySizeRequiredToEncode)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EncoderResources.WebEncoders_InvalidCountOffsetOrLength, "count", "outputOffset", "output"), "count");
		}
		if (count == 0)
		{
			return 0;
		}
		int num = Convert.ToBase64CharArray(input, offset, count, output, outputOffset);
		for (int i = outputOffset; i - outputOffset < num; i++)
		{
			switch (output[i])
			{
			case '+':
				output[i] = '-';
				break;
			case '/':
				output[i] = '_';
				break;
			case '=':
				return i - outputOffset;
			}
		}
		return num;
	}

	public static int GetArraySizeRequiredToEncode(int count)
	{
		checked
		{
			return unchecked(checked(count + 2) / 3) * 4;
		}
	}

	private static int GetNumBase64PaddingCharsInString(string str)
	{
		if (str[str.Length - 1] == '=')
		{
			if (str[str.Length - 2] == '=')
			{
				return 2;
			}
			return 1;
		}
		return 0;
	}

	private static int GetNumBase64PaddingCharsToAddForDecode(int inputLength)
	{
		return (inputLength % 4) switch
		{
			0 => 0, 
			2 => 2, 
			3 => 1, 
			_ => throw new FormatException(string.Format(CultureInfo.CurrentCulture, EncoderResources.WebEncoders_MalformedInput, inputLength)), 
		};
	}

	private static void ValidateParameters(int bufferLength, string inputName, int offset, int count)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (bufferLength - offset < count)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EncoderResources.WebEncoders_InvalidCountOffsetOrLength, "count", "offset", inputName), "count");
		}
	}
}
