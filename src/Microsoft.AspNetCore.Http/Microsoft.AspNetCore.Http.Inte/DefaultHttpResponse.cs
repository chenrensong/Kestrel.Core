using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http.Internal;

public class DefaultHttpResponse : HttpResponse
{
	private struct FeatureInterfaces
	{
		public IHttpResponseFeature Response;

		public IResponseCookiesFeature Cookies;
	}

	private static readonly Func<IFeatureCollection, IHttpResponseFeature> _nullResponseFeature = (IFeatureCollection f) => null;

	private static readonly Func<IFeatureCollection, IResponseCookiesFeature> _newResponseCookiesFeature = (IFeatureCollection f) => new ResponseCookiesFeature(f);

	private HttpContext _context;

	private FeatureReferences<FeatureInterfaces> _features;

	private IHttpResponseFeature HttpResponseFeature => _features.Fetch(ref _features.Cache.Response, _nullResponseFeature);

	private IResponseCookiesFeature ResponseCookiesFeature => _features.Fetch(ref _features.Cache.Cookies, _newResponseCookiesFeature);

	public override HttpContext HttpContext => _context;

	public override int StatusCode
	{
		get
		{
			return HttpResponseFeature.StatusCode;
		}
		set
		{
			HttpResponseFeature.StatusCode = value;
		}
	}

	public override IHeaderDictionary Headers => HttpResponseFeature.Headers;

	public override Stream Body
	{
		get
		{
			return HttpResponseFeature.Body;
		}
		set
		{
			HttpResponseFeature.Body = value;
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

	public override string ContentType
	{
		get
		{
			return Headers["Content-Type"];
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				HttpResponseFeature.Headers.Remove("Content-Type");
			}
			else
			{
				HttpResponseFeature.Headers["Content-Type"] = value;
			}
		}
	}

	public override IResponseCookies Cookies => ResponseCookiesFeature.Cookies;

	public override bool HasStarted => HttpResponseFeature.HasStarted;

	public DefaultHttpResponse(HttpContext context)
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

	public override void OnStarting(Func<object, Task> callback, object state)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		HttpResponseFeature.OnStarting(callback, state);
	}

	public override void OnCompleted(Func<object, Task> callback, object state)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		HttpResponseFeature.OnCompleted(callback, state);
	}

	public override void Redirect(string location, bool permanent)
	{
		if (permanent)
		{
			HttpResponseFeature.StatusCode = 301;
		}
		else
		{
			HttpResponseFeature.StatusCode = 302;
		}
		Headers["Location"] = location;
	}
}
