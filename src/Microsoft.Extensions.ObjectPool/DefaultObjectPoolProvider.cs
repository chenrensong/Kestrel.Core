using System;

namespace Microsoft.Extensions.ObjectPool;

public class DefaultObjectPoolProvider : ObjectPoolProvider
{
	public int MaximumRetained { get; set; } = Environment.ProcessorCount * 2;


	public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
	{
		return new DefaultObjectPool<T>(policy, MaximumRetained);
	}
}
