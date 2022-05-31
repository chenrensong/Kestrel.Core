using System;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.WebUtilities;

public class FormMultipartSection
{
	private ContentDispositionHeaderValue _contentDispositionHeader;

	public MultipartSection Section { get; }

	public string Name { get; }

	public FormMultipartSection(MultipartSection section)
		: this(section, section.GetContentDispositionHeader())
	{
	}

	public FormMultipartSection(MultipartSection section, ContentDispositionHeaderValue header)
	{
		if (header == null || !header.IsFormDisposition())
		{
			throw new ArgumentException("Argument must be a form section", "section");
		}
		Section = section;
		_contentDispositionHeader = header;
		Name = HeaderUtilities.RemoveQuotes(_contentDispositionHeader.Name).ToString();
	}

	public Task<string> GetValueAsync()
	{
		return Section.ReadAsStringAsync();
	}
}
