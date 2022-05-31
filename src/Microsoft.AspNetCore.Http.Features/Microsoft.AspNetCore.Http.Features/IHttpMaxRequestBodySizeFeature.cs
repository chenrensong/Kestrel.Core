namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpMaxRequestBodySizeFeature
{
	bool IsReadOnly { get; }

	long? MaxRequestBodySize { get; set; }
}
