using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebUtilities;

public class MultipartSection
{
	public string ContentType
	{
		get
		{
			if (Headers.TryGetValue("Content-Type", out var value))
			{
				return value;
			}
			return null;
		}
	}

	public string ContentDisposition
	{
		get
		{
			if (Headers.TryGetValue("Content-Disposition", out var value))
			{
				return value;
			}
			return null;
		}
	}

	public Dictionary<string, StringValues> Headers { get; set; }

	public Stream Body { get; set; }

	public long? BaseStreamOffset { get; set; }
}
