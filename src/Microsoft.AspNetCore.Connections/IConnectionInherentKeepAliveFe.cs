namespace Microsoft.AspNetCore.Connections.Features;

public interface IConnectionInherentKeepAliveFeature
{
	bool HasInherentKeepAlive { get; }
}
