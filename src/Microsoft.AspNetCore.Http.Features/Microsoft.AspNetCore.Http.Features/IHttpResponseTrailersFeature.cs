namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpResponseTrailersFeature
{
	IHeaderDictionary Trailers { get; set; }
}
