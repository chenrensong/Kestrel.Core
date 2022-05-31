namespace Microsoft.AspNetCore.Http.Features;

public interface IQueryFeature
{
	IQueryCollection Query { get; set; }
}
