using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http;

public abstract class HttpContext
{
	public abstract IFeatureCollection Features { get; }

	public abstract HttpRequest Request { get; }

	public abstract HttpResponse Response { get; }

	public abstract ConnectionInfo Connection { get; }

	public abstract WebSocketManager WebSockets { get; }

	[Obsolete("This is obsolete and will be removed in a future version. The recommended alternative is to use Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions. See https://go.microsoft.com/fwlink/?linkid=845470.")]
	public abstract AuthenticationManager Authentication { get; }

	public abstract ClaimsPrincipal User { get; set; }

	public abstract IDictionary<object, object> Items { get; set; }

	public abstract IServiceProvider RequestServices { get; set; }

	public abstract CancellationToken RequestAborted { get; set; }

	public abstract string TraceIdentifier { get; set; }

	public abstract ISession Session { get; set; }

	public abstract void Abort();
}
