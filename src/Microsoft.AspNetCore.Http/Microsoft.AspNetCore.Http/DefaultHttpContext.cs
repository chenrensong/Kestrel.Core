using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Authentication.Internal;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Http.Internal;

namespace Microsoft.AspNetCore.Http;

public class DefaultHttpContext : HttpContext
{
	private struct FeatureInterfaces
	{
		public IItemsFeature Items;

		public IServiceProvidersFeature ServiceProviders;

		public IHttpAuthenticationFeature Authentication;

		public IHttpRequestLifetimeFeature Lifetime;

		public ISessionFeature Session;

		public IHttpRequestIdentifierFeature RequestIdentifier;
	}

	private static readonly Func<IFeatureCollection, IItemsFeature> _newItemsFeature = (IFeatureCollection f) => new ItemsFeature();

	private static readonly Func<IFeatureCollection, IServiceProvidersFeature> _newServiceProvidersFeature = (IFeatureCollection f) => new ServiceProvidersFeature();

	private static readonly Func<IFeatureCollection, IHttpAuthenticationFeature> _newHttpAuthenticationFeature = (IFeatureCollection f) => new HttpAuthenticationFeature();

	private static readonly Func<IFeatureCollection, IHttpRequestLifetimeFeature> _newHttpRequestLifetimeFeature = (IFeatureCollection f) => new HttpRequestLifetimeFeature();

	private static readonly Func<IFeatureCollection, ISessionFeature> _newSessionFeature = (IFeatureCollection f) => new DefaultSessionFeature();

	private static readonly Func<IFeatureCollection, ISessionFeature> _nullSessionFeature = (IFeatureCollection f) => null;

	private static readonly Func<IFeatureCollection, IHttpRequestIdentifierFeature> _newHttpRequestIdentifierFeature = (IFeatureCollection f) => new HttpRequestIdentifierFeature();

	private FeatureReferences<FeatureInterfaces> _features;

	private HttpRequest _request;

	private HttpResponse _response;

	private AuthenticationManager _authenticationManager;

	private ConnectionInfo _connection;

	private WebSocketManager _websockets;

	private IItemsFeature ItemsFeature => _features.Fetch(ref _features.Cache.Items, _newItemsFeature);

	private IServiceProvidersFeature ServiceProvidersFeature => _features.Fetch(ref _features.Cache.ServiceProviders, _newServiceProvidersFeature);

	private IHttpAuthenticationFeature HttpAuthenticationFeature => _features.Fetch(ref _features.Cache.Authentication, _newHttpAuthenticationFeature);

	private IHttpRequestLifetimeFeature LifetimeFeature => _features.Fetch(ref _features.Cache.Lifetime, _newHttpRequestLifetimeFeature);

	private ISessionFeature SessionFeature => _features.Fetch(ref _features.Cache.Session, _newSessionFeature);

	private ISessionFeature SessionFeatureOrNull => _features.Fetch(ref _features.Cache.Session, _nullSessionFeature);

	private IHttpRequestIdentifierFeature RequestIdentifierFeature => _features.Fetch(ref _features.Cache.RequestIdentifier, _newHttpRequestIdentifierFeature);

	public override IFeatureCollection Features => _features.Collection;

	public override HttpRequest Request => _request;

	public override HttpResponse Response => _response;

	public override ConnectionInfo Connection => _connection ?? (_connection = InitializeConnectionInfo());

	[Obsolete("This is obsolete and will be removed in a future version. The recommended alternative is to use Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions. See https://go.microsoft.com/fwlink/?linkid=845470.")]
	public override AuthenticationManager Authentication => _authenticationManager ?? (_authenticationManager = InitializeAuthenticationManager());

	public override WebSocketManager WebSockets => _websockets ?? (_websockets = InitializeWebSocketManager());

	public override ClaimsPrincipal User
	{
		get
		{
			ClaimsPrincipal claimsPrincipal = HttpAuthenticationFeature.User;
			if (claimsPrincipal == null)
			{
				claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
				HttpAuthenticationFeature.User = claimsPrincipal;
			}
			return claimsPrincipal;
		}
		set
		{
			HttpAuthenticationFeature.User = value;
		}
	}

	public override IDictionary<object, object> Items
	{
		get
		{
			return ItemsFeature.Items;
		}
		set
		{
			ItemsFeature.Items = value;
		}
	}

	public override IServiceProvider RequestServices
	{
		get
		{
			return ServiceProvidersFeature.RequestServices;
		}
		set
		{
			ServiceProvidersFeature.RequestServices = value;
		}
	}

	public override CancellationToken RequestAborted
	{
		get
		{
			return LifetimeFeature.RequestAborted;
		}
		set
		{
			LifetimeFeature.RequestAborted = value;
		}
	}

	public override string TraceIdentifier
	{
		get
		{
			return RequestIdentifierFeature.TraceIdentifier;
		}
		set
		{
			RequestIdentifierFeature.TraceIdentifier = value;
		}
	}

	public override ISession Session
	{
		get
		{
			return (SessionFeatureOrNull ?? throw new InvalidOperationException("Session has not been configured for this application or request.")).Session;
		}
		set
		{
			SessionFeature.Session = value;
		}
	}

	public DefaultHttpContext()
		: this(new FeatureCollection())
	{
		Features.Set((IHttpRequestFeature)new HttpRequestFeature());
		Features.Set((IHttpResponseFeature)new HttpResponseFeature());
	}

	public DefaultHttpContext(IFeatureCollection features)
	{
		Initialize(features);
	}

	public virtual void Initialize(IFeatureCollection features)
	{
		_features = new FeatureReferences<FeatureInterfaces>(features);
		_request = InitializeHttpRequest();
		_response = InitializeHttpResponse();
	}

	public virtual void Uninitialize()
	{
		_features = default(FeatureReferences<FeatureInterfaces>);
		if (_request != null)
		{
			UninitializeHttpRequest(_request);
			_request = null;
		}
		if (_response != null)
		{
			UninitializeHttpResponse(_response);
			_response = null;
		}
		if (_authenticationManager != null)
		{
			UninitializeAuthenticationManager(_authenticationManager);
			_authenticationManager = null;
		}
		if (_connection != null)
		{
			UninitializeConnectionInfo(_connection);
			_connection = null;
		}
		if (_websockets != null)
		{
			UninitializeWebSocketManager(_websockets);
			_websockets = null;
		}
	}

	public override void Abort()
	{
		LifetimeFeature.Abort();
	}

	protected virtual HttpRequest InitializeHttpRequest()
	{
		return new DefaultHttpRequest(this);
	}

	protected virtual void UninitializeHttpRequest(HttpRequest instance)
	{
	}

	protected virtual HttpResponse InitializeHttpResponse()
	{
		return new DefaultHttpResponse(this);
	}

	protected virtual void UninitializeHttpResponse(HttpResponse instance)
	{
	}

	protected virtual ConnectionInfo InitializeConnectionInfo()
	{
		return new DefaultConnectionInfo(Features);
	}

	protected virtual void UninitializeConnectionInfo(ConnectionInfo instance)
	{
	}

	[Obsolete("This is obsolete and will be removed in a future version. See https://go.microsoft.com/fwlink/?linkid=845470.")]
	protected virtual AuthenticationManager InitializeAuthenticationManager()
	{
		return new DefaultAuthenticationManager(this);
	}

	[Obsolete("This is obsolete and will be removed in a future version. See https://go.microsoft.com/fwlink/?linkid=845470.")]
	protected virtual void UninitializeAuthenticationManager(AuthenticationManager instance)
	{
	}

	protected virtual WebSocketManager InitializeWebSocketManager()
	{
		return new DefaultWebSocketManager(Features);
	}

	protected virtual void UninitializeWebSocketManager(WebSocketManager instance)
	{
	}
}
