using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http.Internal;

public class DefaultConnectionInfo : ConnectionInfo
{
	private struct FeatureInterfaces
	{
		public IHttpConnectionFeature Connection;

		public ITlsConnectionFeature TlsConnection;
	}

	private static readonly Func<IFeatureCollection, IHttpConnectionFeature> _newHttpConnectionFeature = (IFeatureCollection f) => new HttpConnectionFeature();

	private static readonly Func<IFeatureCollection, ITlsConnectionFeature> _newTlsConnectionFeature = (IFeatureCollection f) => new TlsConnectionFeature();

	private FeatureReferences<FeatureInterfaces> _features;

	private IHttpConnectionFeature HttpConnectionFeature => _features.Fetch(ref _features.Cache.Connection, _newHttpConnectionFeature);

	private ITlsConnectionFeature TlsConnectionFeature => _features.Fetch(ref _features.Cache.TlsConnection, _newTlsConnectionFeature);

	public override string Id
	{
		get
		{
			return HttpConnectionFeature.ConnectionId;
		}
		set
		{
			HttpConnectionFeature.ConnectionId = value;
		}
	}

	public override IPAddress RemoteIpAddress
	{
		get
		{
			return HttpConnectionFeature.RemoteIpAddress;
		}
		set
		{
			HttpConnectionFeature.RemoteIpAddress = value;
		}
	}

	public override int RemotePort
	{
		get
		{
			return HttpConnectionFeature.RemotePort;
		}
		set
		{
			HttpConnectionFeature.RemotePort = value;
		}
	}

	public override IPAddress LocalIpAddress
	{
		get
		{
			return HttpConnectionFeature.LocalIpAddress;
		}
		set
		{
			HttpConnectionFeature.LocalIpAddress = value;
		}
	}

	public override int LocalPort
	{
		get
		{
			return HttpConnectionFeature.LocalPort;
		}
		set
		{
			HttpConnectionFeature.LocalPort = value;
		}
	}

	public override X509Certificate2 ClientCertificate
	{
		get
		{
			return TlsConnectionFeature.ClientCertificate;
		}
		set
		{
			TlsConnectionFeature.ClientCertificate = value;
		}
	}

	public DefaultConnectionInfo(IFeatureCollection features)
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

	public override Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return TlsConnectionFeature.GetClientCertificateAsync(cancellationToken);
	}
}
