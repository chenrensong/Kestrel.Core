using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Connections;

public class ConnectionBuilder : IConnectionBuilder
{
	private readonly IList<Func<ConnectionDelegate, ConnectionDelegate>> _components = new List<Func<ConnectionDelegate, ConnectionDelegate>>();

	public IServiceProvider ApplicationServices { get; }

	public ConnectionBuilder(IServiceProvider applicationServices)
	{
		ApplicationServices = applicationServices;
	}

	public IConnectionBuilder Use(Func<ConnectionDelegate, ConnectionDelegate> middleware)
	{
		_components.Add(middleware);
		return this;
	}

	public ConnectionDelegate Build()
	{
		ConnectionDelegate connectionDelegate = (ConnectionContext features) => Task.CompletedTask;
		foreach (Func<ConnectionDelegate, ConnectionDelegate> item in _components.Reverse())
		{
			connectionDelegate = item(connectionDelegate);
		}
		return connectionDelegate;
	}


}
