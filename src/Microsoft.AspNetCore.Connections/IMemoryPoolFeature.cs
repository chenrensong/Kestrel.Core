using System.Buffers;

namespace Microsoft.AspNetCore.Connections.Features;

public interface IMemoryPoolFeature
{
	MemoryPool<byte> MemoryPool { get; }
}
