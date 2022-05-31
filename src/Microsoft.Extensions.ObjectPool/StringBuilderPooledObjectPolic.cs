using System.Text;

namespace Microsoft.Extensions.ObjectPool;

public class StringBuilderPooledObjectPolicy : PooledObjectPolicy<StringBuilder>
{
	public int InitialCapacity { get; set; } = 100;


	public int MaximumRetainedCapacity { get; set; } = 4096;


	public override StringBuilder Create()
	{
		return new StringBuilder(InitialCapacity);
	}

	public override bool Return(StringBuilder obj)
	{
		if (obj.Capacity > MaximumRetainedCapacity)
		{
			return false;
		}
		obj.Clear();
		return true;
	}
}
