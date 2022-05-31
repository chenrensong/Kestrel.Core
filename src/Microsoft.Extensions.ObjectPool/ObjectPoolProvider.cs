namespace Microsoft.Extensions.ObjectPool;

public abstract class ObjectPoolProvider
{
	public ObjectPool<T> Create<T>() where T : class, new()
	{
		return Create(new DefaultPooledObjectPolicy<T>());
	}

	public abstract ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy) where T : class;
}
