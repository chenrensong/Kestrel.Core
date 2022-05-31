using System.Collections.Generic;

namespace Microsoft.AspNetCore.Hosting.Server.Features;

public interface IServerAddressesFeature
{
	ICollection<string> Addresses { get; }

	bool PreferHostingUrls { get; set; }
}
