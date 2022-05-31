using System;

namespace Microsoft.Extensions.ObjectPool;

public class LeakTrackingObjectPoolProvider : ObjectPoolProvider
{
	private readonly ObjectPoolProvider _inner;

	public LeakTrackingObjectPoolProvider(ObjectPoolProvider inner)
	{
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		_inner = inner;
	}

	public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
	{
		return new LeakTrackingObjectPool<T>(_inner.Create(policy));
	}
}
