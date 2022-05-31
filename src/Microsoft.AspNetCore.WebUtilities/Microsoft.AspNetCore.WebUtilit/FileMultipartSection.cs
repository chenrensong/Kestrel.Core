using System;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.WebUtilities;

public class FileMultipartSection
{
	private ContentDispositionHeaderValue _contentDispositionHeader;

	public MultipartSection Section { get; }

	public Stream FileStream => Section.Body;

	public string Name { get; }

	public string FileName { get; }

	public FileMultipartSection(MultipartSection section)
		: this(section, section.GetContentDispositionHeader())
	{
	}

	public FileMultipartSection(MultipartSection section, ContentDispositionHeaderValue header)
	{
		if (!header.IsFileDisposition())
		{
			throw new ArgumentException("Argument must be a file section", "section");
		}
		Section = section;
		_contentDispositionHeader = header;
		Name = HeaderUtilities.RemoveQuotes(_contentDispositionHeader.Name).ToString();
		FileName = HeaderUtilities.RemoveQuotes(_contentDispositionHeader.FileNameStar.HasValue ? _contentDispositionHeader.FileNameStar : _contentDispositionHeader.FileName).ToString();
	}
}
