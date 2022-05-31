using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.WebUtilities;

public static class MultipartSectionStreamExtensions
{
	public static async Task<string> ReadAsStringAsync(this MultipartSection section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		MediaTypeHeaderValue.TryParse(section.ContentType, out var parsedValue);
		Encoding encoding = parsedValue?.Encoding;
		if (encoding == null || encoding == Encoding.UTF7)
		{
			encoding = Encoding.UTF8;
		}
		using StreamReader reader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
		return await reader.ReadToEndAsync();
	}
}
