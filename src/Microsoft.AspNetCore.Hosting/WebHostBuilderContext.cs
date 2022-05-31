using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.Hosting;

public class WebHostBuilderContext
{
	public IHostEnvironment HostingEnvironment { get; set; }

	public IConfiguration Configuration { get; set; }
}
