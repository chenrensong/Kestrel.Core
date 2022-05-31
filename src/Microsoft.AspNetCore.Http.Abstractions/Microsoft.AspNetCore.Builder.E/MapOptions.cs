using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder.Extensions;

public class MapOptions
{
	public PathString PathMatch { get; set; }

	public RequestDelegate Branch { get; set; }
}
