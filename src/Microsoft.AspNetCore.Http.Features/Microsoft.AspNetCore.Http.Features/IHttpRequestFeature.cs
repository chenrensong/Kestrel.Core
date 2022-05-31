using System.IO;

namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpRequestFeature
{
	string Protocol { get; set; }

	string Scheme { get; set; }

	string Method { get; set; }

	string PathBase { get; set; }

	string Path { get; set; }

	string QueryString { get; set; }

	string RawTarget { get; set; }

	IHeaderDictionary Headers { get; set; }

	Stream Body { get; set; }
}
