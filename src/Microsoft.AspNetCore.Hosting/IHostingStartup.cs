namespace Microsoft.AspNetCore.Hosting;

public interface IHostingStartup
{
	void Configure(IWebHostBuilder builder);
}
