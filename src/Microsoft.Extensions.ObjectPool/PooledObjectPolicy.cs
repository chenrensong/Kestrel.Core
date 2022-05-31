namespace Microsoft.Extensions.ObjectPool;

public abstract class PooledObjectPolicy<T> : IPooledObjectPolicy<T>
{
	public abstract T Create();

	public abstract bool Return(T obj);
}
