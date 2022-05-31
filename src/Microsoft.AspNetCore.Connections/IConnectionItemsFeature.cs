using System.Collections.Generic;

namespace Microsoft.AspNetCore.Connections.Features;

public interface IConnectionItemsFeature
{
	IDictionary<object, object> Items { get; set; }
}
