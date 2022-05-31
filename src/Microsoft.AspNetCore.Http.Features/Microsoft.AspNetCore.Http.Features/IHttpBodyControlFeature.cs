namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpBodyControlFeature
{
	bool AllowSynchronousIO { get; set; }
}
