namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpRequestIdentifierFeature
{
	string TraceIdentifier { get; set; }
}
