using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Internal;

namespace Microsoft.AspNetCore.Http.Features;

public class ItemsFeature : IItemsFeature
{
	public IDictionary<object, object> Items { get; set; }

	public ItemsFeature()
	{
		Items = new ItemsDictionary();
	}
}
