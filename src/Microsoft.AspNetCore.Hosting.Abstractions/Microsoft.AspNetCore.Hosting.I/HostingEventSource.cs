using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.Hosting.Internal;

[EventSource(Name = "Microsoft-AspNetCore-Hosting")]
public sealed class HostingEventSource : EventSource
{
	public static readonly HostingEventSource Log = new HostingEventSource();

	private HostingEventSource()
	{
	}

	[Event(1, Level = EventLevel.Informational)]
	public void HostStart()
	{
		WriteEvent(1);
	}

	[Event(2, Level = EventLevel.Informational)]
	public void HostStop()
	{
		WriteEvent(2);
	}

	[Event(3, Level = EventLevel.Informational)]
	public void RequestStart(string method, string path)
	{
		WriteEvent(3, method, path);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Event(4, Level = EventLevel.Informational)]
	public void RequestStop()
	{
		WriteEvent(4);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Event(5, Level = EventLevel.Error)]
	public void UnhandledException()
	{
		WriteEvent(5);
	}
}
