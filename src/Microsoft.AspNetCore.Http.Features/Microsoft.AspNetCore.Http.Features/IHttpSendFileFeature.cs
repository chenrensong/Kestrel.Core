using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpSendFileFeature
{
	Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellation);
}
