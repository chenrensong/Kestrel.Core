using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Connections;

public class DefaultConnectionContext : ConnectionContext,
    IDisposable, IConnectionIdFeature, IConnectionItemsFeature,
    IConnectionTransportFeature, IConnectionUserFeature, IConnectionLifetimeFeature
    , IHttpContextFeature
{
    private CancellationTokenSource _connectionClosedTokenSource = new CancellationTokenSource();

    public override string ConnectionId { get; set; }

    public override IFeatureCollection Features { get; }

    public ClaimsPrincipal User { get; set; }

    public override IDictionary<object, object> Items { get; set; } = new ConnectionItems();

    public IDuplexPipe Application { get; set; }

    public override IDuplexPipe Transport { get; set; }

    public CancellationToken ConnectionClosed { get; set; }
    public HttpContext HttpContext { get; set; }

    public DefaultConnectionContext()
        : this(Guid.NewGuid().ToString())
    {
        ConnectionClosed = _connectionClosedTokenSource.Token;
    }

    public DefaultConnectionContext(string id)
    {
        ConnectionId = id;
        Features = new FeatureCollection();
        Features.Set((IConnectionUserFeature)this);
        Features.Set((IConnectionItemsFeature)this);
        Features.Set((IConnectionIdFeature)this);
        Features.Set((IConnectionTransportFeature)this);
        Features.Set((IConnectionLifetimeFeature)this);
        Features.Set((IHttpContextFeature)this);
    }

    public DefaultConnectionContext(string id, IDuplexPipe transport, IDuplexPipe application)
        : this(id)
    {
        Transport = transport;
        Application = application;
    }

    public override void Abort(ConnectionAbortedException abortReason)
    {
        ThreadPool.UnsafeQueueUserWorkItem(delegate (object cts)
        {
            ((CancellationTokenSource)cts).Cancel();
        }, _connectionClosedTokenSource);
    }

    public void Dispose()
    {
        _connectionClosedTokenSource.Dispose();
    }
}
