using System;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Hosting;

public interface IStartupFilter
{
	Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next);
}
