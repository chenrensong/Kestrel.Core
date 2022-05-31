using System.Collections.Generic;

namespace Microsoft.AspNetCore.Http.Features;

public interface IItemsFeature
{
	IDictionary<object, object> Items { get; set; }
}
