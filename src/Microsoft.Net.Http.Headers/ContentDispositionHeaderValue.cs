using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers;

public class ContentDispositionHeaderValue
{
	private const string FileNameString = "filename";

	private const string NameString = "name";

	private const string FileNameStarString = "filename*";

	private const string CreationDateString = "creation-date";

	private const string ModificationDateString = "modification-date";

	private const string ReadDateString = "read-date";

	private const string SizeString = "size";

	private static readonly char[] QuestionMark = new char[1] { '?' };

	private static readonly char[] SingleQuote = new char[1] { '\'' };

	private static readonly HttpHeaderParser<ContentDispositionHeaderValue> Parser = new GenericHeaderParser<ContentDispositionHeaderValue>(supportsMultipleValues: false, GetDispositionTypeLength);

	private ObjectCollection<NameValueHeaderValue> _parameters;

	private StringSegment _dispositionType;

	private static readonly char[] HexUpperChars = new char[16]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F'
	};

	public StringSegment DispositionType
	{
		get
		{
			return _dispositionType;
		}
		set
		{
			CheckDispositionTypeFormat(value, "value");
			_dispositionType = value;
		}
	}

	public IList<NameValueHeaderValue> Parameters
	{
		get
		{
			if (_parameters == null)
			{
				_parameters = new ObjectCollection<NameValueHeaderValue>();
			}
			return _parameters;
		}
	}

	public StringSegment Name
	{
		get
		{
			return GetName("name");
		}
		set
		{
			SetName("name", value);
		}
	}

	public StringSegment FileName
	{
		get
		{
			return GetName("filename");
		}
		set
		{
			SetName("filename", value);
		}
	}

	public StringSegment FileNameStar
	{
		get
		{
			return GetName("filename*");
		}
		set
		{
			SetName("filename*", value);
		}
	}

	public DateTimeOffset? CreationDate
	{
		get
		{
			return GetDate("creation-date");
		}
		set
		{
			SetDate("creation-date", value);
		}
	}

	public DateTimeOffset? ModificationDate
	{
		get
		{
			return GetDate("modification-date");
		}
		set
		{
			SetDate("modification-date", value);
		}
	}

	public DateTimeOffset? ReadDate
	{
		get
		{
			return GetDate("read-date");
		}
		set
		{
			SetDate("read-date", value);
		}
	}

	public long? Size
	{
		get
		{
			NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, "size");
			if (nameValueHeaderValue != null && HeaderUtilities.TryParseNonNegativeInt64(nameValueHeaderValue.Value, out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, "size");
			if (!value.HasValue)
			{
				if (nameValueHeaderValue != null)
				{
					_parameters.Remove(nameValueHeaderValue);
				}
				return;
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (nameValueHeaderValue != null)
			{
				nameValueHeaderValue.Value = value.Value.ToString(CultureInfo.InvariantCulture);
				return;
			}
			string text = value.Value.ToString(CultureInfo.InvariantCulture);
			_parameters.Add(new NameValueHeaderValue("size", text));
		}
	}

	private ContentDispositionHeaderValue()
	{
	}

	public ContentDispositionHeaderValue(StringSegment dispositionType)
	{
		CheckDispositionTypeFormat(dispositionType, "dispositionType");
		_dispositionType = dispositionType;
	}

	public void SetHttpFileName(StringSegment fileName)
	{
		if (!StringSegment.IsNullOrEmpty(fileName))
		{
			FileName = Sanitize(fileName);
		}
		else
		{
			FileName = fileName;
		}
		FileNameStar = fileName;
	}

	public void SetMimeFileName(StringSegment fileName)
	{
		FileNameStar = null;
		FileName = fileName;
	}

	public override string ToString()
	{
		return string.Concat((object?)_dispositionType,
			NameValueHeaderValue.ToString(_parameters, ';', leadingSeparator: true));
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ContentDispositionHeaderValue contentDispositionHeaderValue))
		{
			return false;
		}
		if (_dispositionType.Equals(contentDispositionHeaderValue._dispositionType, StringComparison.OrdinalIgnoreCase))
		{
			return HeaderUtilities.AreEqualCollections(_parameters, contentDispositionHeaderValue._parameters);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_dispositionType) ^ NameValueHeaderValue.GetHashCode(_parameters);
	}

	public static ContentDispositionHeaderValue Parse(StringSegment input)
	{
		int index = 0;
		return Parser.ParseValue(input, ref index);
	}

	public static bool TryParse(StringSegment input, out ContentDispositionHeaderValue parsedValue)
	{
		int index = 0;
		return Parser.TryParseValue(input, ref index, out parsedValue);
	}

	private static int GetDispositionTypeLength(StringSegment input, int startIndex, out ContentDispositionHeaderValue parsedValue)
	{
		parsedValue = null;
		if (StringSegment.IsNullOrEmpty(input) || startIndex >= input.Length)
		{
			return 0;
		}
		StringSegment dispositionType;
		int dispositionTypeExpressionLength = GetDispositionTypeExpressionLength(input, startIndex, out dispositionType);
		if (dispositionTypeExpressionLength == 0)
		{
			return 0;
		}
		int num = startIndex + dispositionTypeExpressionLength;
		num += HttpRuleParser.GetWhitespaceLength(input, num);
		ContentDispositionHeaderValue contentDispositionHeaderValue = new ContentDispositionHeaderValue();
		contentDispositionHeaderValue._dispositionType = dispositionType;
		if (num < input.Length && input[num] == ';')
		{
			num++;
			int nameValueListLength = NameValueHeaderValue.GetNameValueListLength(input, num, ';', contentDispositionHeaderValue.Parameters);
			parsedValue = contentDispositionHeaderValue;
			return num + nameValueListLength - startIndex;
		}
		parsedValue = contentDispositionHeaderValue;
		return num - startIndex;
	}

	private static int GetDispositionTypeExpressionLength(StringSegment input, int startIndex, out StringSegment dispositionType)
	{
		dispositionType = null;
		int tokenLength = HttpRuleParser.GetTokenLength(input, startIndex);
		if (tokenLength == 0)
		{
			return 0;
		}
		dispositionType = input.Subsegment(startIndex, tokenLength);
		return tokenLength;
	}

	private static void CheckDispositionTypeFormat(StringSegment dispositionType, string parameterName)
	{
		if (StringSegment.IsNullOrEmpty(dispositionType))
		{
			throw new ArgumentException("An empty string is not allowed.", parameterName);
		}
		if (GetDispositionTypeExpressionLength(dispositionType, 0, out var dispositionType2) == 0 || dispositionType2.Length != dispositionType.Length)
		{
			throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Invalid disposition type '{0}'.", dispositionType));
		}
	}

	private DateTimeOffset? GetDate(string parameter)
	{
		NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, parameter);
		if (nameValueHeaderValue != null)
		{
			StringSegment stringSegment = nameValueHeaderValue.Value;
			if (IsQuoted(stringSegment))
			{
				stringSegment = stringSegment.Subsegment(1, stringSegment.Length - 2);
			}
			if (HttpRuleParser.TryStringToDate(stringSegment, out var result))
			{
				return result;
			}
		}
		return null;
	}

	private void SetDate(string parameter, DateTimeOffset? date)
	{
		NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, parameter);
		if (!date.HasValue)
		{
			if (nameValueHeaderValue != null)
			{
				_parameters.Remove(nameValueHeaderValue);
			}
			return;
		}
		string text = HeaderUtilities.FormatDate(date.Value, quoted: true);
		if (nameValueHeaderValue != null)
		{
			nameValueHeaderValue.Value = text;
		}
		else
		{
			Parameters.Add(new NameValueHeaderValue(parameter, text));
		}
	}

	private StringSegment GetName(string parameter)
	{
		NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, parameter);
		if (nameValueHeaderValue != null)
		{
			string output;
			if (parameter.EndsWith("*", StringComparison.Ordinal))
			{
				if (TryDecode5987(nameValueHeaderValue.Value, out output))
				{
					return output;
				}
				return null;
			}
			if (TryDecodeMime(nameValueHeaderValue.Value, out output))
			{
				return output;
			}
			return HeaderUtilities.RemoveQuotes(nameValueHeaderValue.Value);
		}
		return null;
	}

	private void SetName(StringSegment parameter, StringSegment value)
	{
		NameValueHeaderValue nameValueHeaderValue = NameValueHeaderValue.Find(_parameters, parameter);
		if (StringSegment.IsNullOrEmpty(value))
		{
			if (nameValueHeaderValue != null)
			{
				_parameters.Remove(nameValueHeaderValue);
			}
			return;
		}
		StringSegment empty = StringSegment.Empty;
		empty = ((!parameter.EndsWith("*", StringComparison.Ordinal)) ? EncodeAndQuoteMime(value) : ((StringSegment)Encode5987(value)));
		if (nameValueHeaderValue != null)
		{
			nameValueHeaderValue.Value = empty;
		}
		else
		{
			Parameters.Add(new NameValueHeaderValue(parameter, empty));
		}
	}

	private StringSegment EncodeAndQuoteMime(StringSegment input)
	{
		StringSegment stringSegment = input;
		bool flag = false;
		if (IsQuoted(stringSegment))
		{
			stringSegment = stringSegment.Subsegment(1, stringSegment.Length - 2);
			flag = true;
		}
		if (RequiresEncoding(stringSegment))
		{
			flag = true;
			stringSegment = EncodeMime(stringSegment);
		}
		else if (!flag && HttpRuleParser.GetTokenLength(stringSegment, 0) != stringSegment.Length)
		{
			flag = true;
		}
		if (flag)
		{
			stringSegment = stringSegment.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"");
			stringSegment = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", stringSegment);
		}
		return stringSegment;
	}

	private StringSegment Sanitize(StringSegment input)
	{
		StringSegment stringSegment = input;
		if (RequiresEncoding(stringSegment))
		{
			StringBuilder stringBuilder = new StringBuilder(stringSegment.Length);
			for (int i = 0; i < stringSegment.Length; i++)
			{
				char c = stringSegment[i];
				if (c > '\u007f')
				{
					c = '_';
				}
				stringBuilder.Append(c);
			}
			stringSegment = stringBuilder.ToString();
		}
		return stringSegment;
	}

	private bool IsQuoted(StringSegment value)
	{
		if (value.Length > 1 && value.StartsWith("\"", StringComparison.Ordinal))
		{
			return value.EndsWith("\"", StringComparison.Ordinal);
		}
		return false;
	}

	private bool RequiresEncoding(StringSegment input)
	{
		for (int i = 0; i < input.Length; i++)
		{
			if (input[i] > '\u007f')
			{
				return true;
			}
		}
		return false;
	}

	private unsafe string EncodeMime(StringSegment input)
	{
		fixed (char* ptr = input.Buffer)
		{
			int byteCount = Encoding.UTF8.GetByteCount(ptr + input.Offset, input.Length);
			byte[] array = new byte[byteCount];
			fixed (byte* bytes = array)
			{
				Encoding.UTF8.GetBytes(ptr + input.Offset, input.Length, bytes, byteCount);
			}
			string text = Convert.ToBase64String(array);
			return "=?utf-8?B?" + text + "?=";
		}
	}

	private bool TryDecodeMime(StringSegment input, out string output)
	{
		output = null;
		StringSegment value = input;
		if (!IsQuoted(value) || value.Length < 10)
		{
			return false;
		}
		StringSegment[] array = value.Split(QuestionMark).ToArray();
		if (array.Length != 5 || array[0] != "\"=" || array[4] != "=\"" || !array[2].Equals("b", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		try
		{
			Encoding encoding = Encoding.GetEncoding(array[1].ToString());
			byte[] array2 = Convert.FromBase64String(array[3].ToString());
			output = encoding.GetString(array2, 0, array2.Length);
			return true;
		}
		catch (ArgumentException)
		{
		}
		catch (FormatException)
		{
		}
		return false;
	}

	private string Encode5987(StringSegment input)
	{
		StringBuilder stringBuilder = new StringBuilder("UTF-8''");
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (c > '\u007f')
			{
				byte[] bytes = Encoding.UTF8.GetBytes(c.ToString());
				foreach (byte c2 in bytes)
				{
					HexEscape(stringBuilder, (char)c2);
				}
			}
			else if (!HttpRuleParser.IsTokenChar(c) || c == '*' || c == '\'' || c == '%')
			{
				HexEscape(stringBuilder, c);
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private static void HexEscape(StringBuilder builder, char c)
	{
		builder.Append('%');
		builder.Append(HexUpperChars[(c & 0xF0) >> 4]);
		builder.Append(HexUpperChars[c & 0xF]);
	}

	private bool TryDecode5987(StringSegment input, out string output)
	{
		output = null;
		StringSegment[] array = input.Split(SingleQuote).ToArray();
		if (array.Length != 3)
		{
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = null;
		try
		{
			Encoding encoding = Encoding.GetEncoding(array[0].ToString());
			StringSegment pattern = array[2];
			array2 = ArrayPool<byte>.Shared.Rent(pattern.Length);
			int num = 0;
			for (int i = 0; i < pattern.Length; i++)
			{
				if (IsHexEncoding(pattern, i))
				{
					array2[num++] = HexUnescape(pattern, ref i);
					i--;
					continue;
				}
				if (num > 0)
				{
					stringBuilder.Append(encoding.GetString(array2, 0, num));
					num = 0;
				}
				stringBuilder.Append(pattern[i]);
			}
			if (num > 0)
			{
				stringBuilder.Append(encoding.GetString(array2, 0, num));
			}
		}
		catch (ArgumentException)
		{
			return false;
		}
		finally
		{
			if (array2 != null)
			{
				ArrayPool<byte>.Shared.Return(array2);
			}
		}
		output = stringBuilder.ToString();
		return true;
	}

	private static bool IsHexEncoding(StringSegment pattern, int index)
	{
		if (pattern.Length - index < 3)
		{
			return false;
		}
		if (pattern[index] == '%' && IsEscapedAscii(pattern[index + 1], pattern[index + 2]))
		{
			return true;
		}
		return false;
	}

	private static bool IsEscapedAscii(char digit, char next)
	{
		if ((digit < '0' || digit > '9') && (digit < 'A' || digit > 'F') && (digit < 'a' || digit > 'f'))
		{
			return false;
		}
		if ((next < '0' || next > '9') && (next < 'A' || next > 'F') && (next < 'a' || next > 'f'))
		{
			return false;
		}
		return true;
	}

	private static byte HexUnescape(StringSegment pattern, ref int index)
	{
		if (index < 0 || index >= pattern.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (pattern[index] == '%' && pattern.Length - index >= 3)
		{
			byte result = UnEscapeAscii(pattern[index + 1], pattern[index + 2]);
			index += 3;
			return result;
		}
		return (byte)pattern[index++];
	}

	internal static byte UnEscapeAscii(char digit, char next)
	{
		if ((digit < '0' || digit > '9') && (digit < 'A' || digit > 'F') && (digit < 'a' || digit > 'f'))
		{
			throw new ArgumentException();
		}
		int num = ((digit <= '9') ? (digit - 48) : (((digit <= 'F') ? (digit - 65) : (digit - 97)) + 10));
		if ((next < '0' || next > '9') && (next < 'A' || next > 'F') && (next < 'a' || next > 'f'))
		{
			throw new ArgumentException();
		}
		return (byte)((num << 4) + ((next <= '9') ? (next - 48) : (((next <= 'F') ? (next - 65) : (next - 97)) + 10)));
	}
}
