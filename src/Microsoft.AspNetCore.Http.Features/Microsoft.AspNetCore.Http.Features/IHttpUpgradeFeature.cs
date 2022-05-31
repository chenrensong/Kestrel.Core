using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpUpgradeFeature
{
	bool IsUpgradableRequest { get; }

	Task<Stream> UpgradeAsync();
}
