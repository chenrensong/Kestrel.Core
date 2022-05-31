using System;

namespace Microsoft.AspNetCore.Hosting.Internal;

public interface IStartupConfigureContainerFilter<TContainerBuilder>
{
	Action<TContainerBuilder> ConfigureContainer(Action<TContainerBuilder> container);
}
