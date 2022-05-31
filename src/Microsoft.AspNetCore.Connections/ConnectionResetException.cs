using System;
using System.IO;

namespace Microsoft.AspNetCore.Connections;

public class ConnectionResetException : IOException
{
	public ConnectionResetException(string message)
		: base(message)
	{
	}

	public ConnectionResetException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
