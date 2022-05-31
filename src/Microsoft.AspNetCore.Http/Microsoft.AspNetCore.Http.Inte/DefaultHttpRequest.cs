using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http.Internal;

public class DefaultHttpRequest : HttpRequest
{
	private struct FeatureInterfaces
	{
		public IHttpRequestFeature Request;

		public IQueryFeature Query;

		public IFormFeature Form;

		public IRequestCookiesFeature Cookies;
	}

	private static readonly Func<IFeatureCollection, IHttpRequestFeature> _nullRequestFeature = (IFeatureCollection f) => null;

	private static readonly Func<IFeatureCollection, IQueryFeature> _newQueryFeature = (IFeatureCollection f) => new QueryFeature(f);

	private static readonly Func<HttpRequest, IFormFeature> _newFormFeature = (HttpRequest r) => new FormFeature(r);

	private static readonly Func<IFeatureCollection, IRequestCookiesFeature> _newRequestCookiesFeature = (IFeatureCollection f) => new RequestCookiesFeature(f);

	private HttpContext _context;

	private FeatureReferences<FeatureInterfaces> _features;

	public override HttpContext HttpContext => _context;

	private IHttpRequestFeature HttpRequestFeature => _features.Fetch(ref _features.Cache.Request, _nullRequestFeature);

	private IQueryFeature QueryFeature => _features.Fetch(ref _features.Cache.Query, _newQueryFeature);

	private IFormFeature FormFeature => _features.Fetch(ref _features.Cache.Form, this, _newFormFeature);

	private IRequestCookiesFeature RequestCookiesFeature => _features.Fetch(ref _features.Cache.Cookies, _newRequestCookiesFeature);

	public override PathString PathBase
	{
		get
		{
			return new PathString(HttpRequestFeature.PathBase);
		}
		set
		{
			HttpRequestFeature.PathBase = value.Value;
		}
	}

	public override PathString Path
	{
		get
		{
			return new PathString(HttpRequestFeature.Path);
		}
		set
		{
			HttpRequestFeature.Path = value.Value;
		}
	}

	public override QueryString QueryString
	{
		get
		{
			return new QueryString(HttpRequestFeature.QueryString);
		}
		set
		{
			HttpRequestFeature.QueryString = value.Value;
		}
	}

	public override long? ContentLength
	{
		get
		{
			return Headers.ContentLength;
		}
		set
		{
			Headers.ContentLength = value;
		}
	}

	public override Stream Body
	{
		get
		{
			return HttpRequestFeature.Body;
		}
		set
		{
			HttpRequestFeature.Body = value;
		}
	}

	public override string Method
	{
		get
		{
			return HttpRequestFeature.Method;
		}
		set
		{
			HttpRequestFeature.Method = value;
		}
	}

	public override string Scheme
	{
		get
		{
			return HttpRequestFeature.Scheme;
		}
		set
		{
			HttpRequestFeature.Scheme = value;
		}
	}

	public override bool IsHttps
	{
		get
		{
			return string.Equals("https", Scheme, StringComparison.OrdinalIgnoreCase);
		}
		set
		{
			Scheme = (value ? "https" : "http");
		}
	}

	public override HostString Host
	{
		get
		{
			return HostString.FromUriComponent(Headers["Host"]);
		}
		set
		{
			Headers["Host"] = value.ToUriComponent();
		}
	}

	public override IQueryCollection Query
	{
		get
		{
			return QueryFeature.Query;
		}
		set
		{
			QueryFeature.Query = value;
		}
	}

	public override string Protocol
	{
		get
		{
			return HttpRequestFeature.Protocol;
		}
		set
		{
			HttpRequestFeature.Protocol = value;
		}
	}

	public override IHeaderDictionary Headers => HttpRequestFeature.Headers;

	public override IRequestCookieCollection Cookies
	{
		get
		{
			return RequestCookiesFeature.Cookies;
		}
		set
		{
			RequestCookiesFeature.Cookies = value;
		}
	}

	public override string ContentType
	{
		get
		{
			return Headers["Content-Type"];
		}
		set
		{
			Headers["Content-Type"] = value;
		}
	}

	public override bool HasFormContentType => FormFeature.HasFormContentType;

	public override IFormCollection Form
	{
		get
		{
			return FormFeature.ReadForm();
		}
		set
		{
			FormFeature.Form = value;
		}
	}

	public DefaultHttpRequest(HttpContext context)
	{
		Initialize(context);
	}

	public virtual void Initialize(HttpContext context)
	{
		_context = context;
		_features = new FeatureReferences<FeatureInterfaces>(context.Features);
	}

	public virtual void Uninitialize()
	{
		_context = null;
		_features = default(FeatureReferences<FeatureInterfaces>);
	}

	public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken)
	{
		return FormFeature.ReadFormAsync(cancellationToken);
	}
}
