using System;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.WebUtilities;

public static class MultipartSectionConverterExtensions
{
	public static FileMultipartSection AsFileSection(this MultipartSection section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		try
		{
			return new FileMultipartSection(section);
		}
		catch
		{
			return null;
		}
	}

	public static FormMultipartSection AsFormDataSection(this MultipartSection section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		try
		{
			return new FormMultipartSection(section);
		}
		catch
		{
			return null;
		}
	}

	public static ContentDispositionHeaderValue GetContentDispositionHeader(this MultipartSection section)
	{
		if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var parsedValue))
		{
			return null;
		}
		return parsedValue;
	}
}
