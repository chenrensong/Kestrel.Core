using System.Text;

namespace Microsoft.Extensions.ObjectPool;

public static class ObjectPoolProviderExtensions
{
	public static ObjectPool<StringBuilder> CreateStringBuilderPool(this ObjectPoolProvider provider)
	{
		return provider.Create(new StringBuilderPooledObjectPolicy());
	}

	public static ObjectPool<StringBuilder> CreateStringBuilderPool(this ObjectPoolProvider provider, int initialCapacity, int maximumRetainedCapacity)
	{
		StringBuilderPooledObjectPolicy policy = new StringBuilderPooledObjectPolicy
		{
			InitialCapacity = initialCapacity,
			MaximumRetainedCapacity = maximumRetainedCapacity
		};
		return provider.Create(policy);
	}
}
