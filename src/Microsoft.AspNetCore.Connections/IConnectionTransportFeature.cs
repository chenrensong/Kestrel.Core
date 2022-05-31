using System.IO.Pipelines;

namespace Microsoft.AspNetCore.Connections.Features;

public interface IConnectionTransportFeature
{
	IDuplexPipe Transport { get; set; }
}
