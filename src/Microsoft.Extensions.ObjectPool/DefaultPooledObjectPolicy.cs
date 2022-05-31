namespace Microsoft.Extensions.ObjectPool;

public class DefaultPooledObjectPolicy<T> : PooledObjectPolicy<T> where T : class, new()
{
	public override T Create()
	{
		return new T();
	}

	public override bool Return(T obj)
	{
		return true;
	}
}
