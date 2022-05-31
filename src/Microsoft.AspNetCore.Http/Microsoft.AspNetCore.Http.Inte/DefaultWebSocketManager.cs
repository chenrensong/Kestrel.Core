using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http.Internal;

public class DefaultWebSocketManager : WebSocketManager
{
	private struct FeatureInterfaces
	{
		public IHttpRequestFeature Request;

		public IHttpWebSocketFeature WebSockets;
	}

	private static readonly Func<IFeatureCollection, IHttpRequestFeature> _nullRequestFeature = (IFeatureCollection f) => null;

	private static readonly Func<IFeatureCollection, IHttpWebSocketFeature> _nullWebSocketFeature = (IFeatureCollection f) => null;

	private FeatureReferences<FeatureInterfaces> _features;

	private IHttpRequestFeature HttpRequestFeature => _features.Fetch(ref _features.Cache.Request, _nullRequestFeature);

	private IHttpWebSocketFeature WebSocketFeature => _features.Fetch(ref _features.Cache.WebSockets, _nullWebSocketFeature);

	public override bool IsWebSocketRequest
	{
		get
		{
			if (WebSocketFeature != null)
			{
				return WebSocketFeature.IsWebSocketRequest;
			}
			return false;
		}
	}

	public override IList<string> WebSocketRequestedProtocols => ParsingHelpers.GetHeaderSplit(HttpRequestFeature.Headers, "Sec-WebSocket-Protocol");

	public DefaultWebSocketManager(IFeatureCollection features)
	{
		Initialize(features);
	}

	public virtual void Initialize(IFeatureCollection features)
	{
		_features = new FeatureReferences<FeatureInterfaces>(features);
	}

	public virtual void Uninitialize()
	{
		_features = default(FeatureReferences<FeatureInterfaces>);
	}

	public override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
	{
		if (WebSocketFeature == null)
		{
			throw new NotSupportedException("WebSockets are not supported");
		}
		return WebSocketFeature.AcceptAsync(new WebSocketAcceptContext
		{
			SubProtocol = subProtocol
		});
	}
}
