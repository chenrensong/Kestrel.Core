namespace Microsoft.Extensions.ObjectPool;

public interface IPooledObjectPolicy<T>
{
	T Create();

	bool Return(T obj);
}
