namespace Microsoft.Extensions.ObjectPool;

public abstract class ObjectPool<T> where T : class
{
	public abstract T Get();

	public abstract void Return(T obj);
}
