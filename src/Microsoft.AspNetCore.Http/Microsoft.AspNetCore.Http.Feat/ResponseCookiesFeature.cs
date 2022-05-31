using System;
using System.Text;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.Http.Features;

public class ResponseCookiesFeature : IResponseCookiesFeature
{
	private static readonly Func<IFeatureCollection, IHttpResponseFeature> _nullResponseFeature = (IFeatureCollection f) => null;

	private FeatureReferences<IHttpResponseFeature> _features;

	private IResponseCookies _cookiesCollection;

	private IHttpResponseFeature HttpResponseFeature => _features.Fetch(ref _features.Cache, _nullResponseFeature);

	public IResponseCookies Cookies
	{
		get
		{
			if (_cookiesCollection == null)
			{
				IHeaderDictionary headers = HttpResponseFeature.Headers;
				_cookiesCollection = new ResponseCookies(headers, null);
			}
			return _cookiesCollection;
		}
	}

	public ResponseCookiesFeature(IFeatureCollection features)
		: this(features, null)
	{
	}

	public ResponseCookiesFeature(IFeatureCollection features, ObjectPool<StringBuilder> builderPool)
	{
		if (features == null)
		{
			throw new ArgumentNullException("features");
		}
		_features = new FeatureReferences<IHttpResponseFeature>(features);
	}
}
