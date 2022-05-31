using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http.Features;

public interface IFormFeature
{
	bool HasFormContentType { get; }

	IFormCollection Form { get; set; }

	IFormCollection ReadForm();

	Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken);
}
