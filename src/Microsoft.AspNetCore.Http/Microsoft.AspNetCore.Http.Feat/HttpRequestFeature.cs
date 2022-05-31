using System.IO;

namespace Microsoft.AspNetCore.Http.Features;

public class HttpRequestFeature : IHttpRequestFeature
{
	public string Protocol { get; set; }

	public string Scheme { get; set; }

	public string Method { get; set; }

	public string PathBase { get; set; }

	public string Path { get; set; }

	public string QueryString { get; set; }

	public string RawTarget { get; set; }

	public IHeaderDictionary Headers { get; set; }

	public Stream Body { get; set; }

	public HttpRequestFeature()
	{
		Headers = new HeaderDictionary();
		Body = Stream.Null;
		Protocol = string.Empty;
		Scheme = string.Empty;
		Method = string.Empty;
		PathBase = string.Empty;
		Path = string.Empty;
		QueryString = string.Empty;
		RawTarget = string.Empty;
	}
}
