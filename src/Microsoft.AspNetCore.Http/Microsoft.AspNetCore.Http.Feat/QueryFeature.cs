using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http.Features;

public class QueryFeature : IQueryFeature
{
	private static readonly Func<IFeatureCollection, IHttpRequestFeature> _nullRequestFeature = (IFeatureCollection f) => null;

	private FeatureReferences<IHttpRequestFeature> _features;

	private string _original;

	private IQueryCollection _parsedValues;

	private IHttpRequestFeature HttpRequestFeature => _features.Fetch(ref _features.Cache, _nullRequestFeature);

	public IQueryCollection Query
	{
		get
		{
			if (_features.Collection == null)
			{
				if (_parsedValues == null)
				{
					_parsedValues = QueryCollection.Empty;
				}
				return _parsedValues;
			}
			string queryString = HttpRequestFeature.QueryString;
			if (_parsedValues == null || !string.Equals(_original, queryString, StringComparison.Ordinal))
			{
				_original = queryString;
				Dictionary<string, StringValues> dictionary = QueryHelpers.ParseNullableQuery(queryString);
				if (dictionary == null)
				{
					_parsedValues = QueryCollection.Empty;
				}
				else
				{
					_parsedValues = new QueryCollection(dictionary);
				}
			}
			return _parsedValues;
		}
		set
		{
			_parsedValues = value;
			if (_features.Collection != null)
			{
				if (value == null)
				{
					_original = string.Empty;
					HttpRequestFeature.QueryString = string.Empty;
				}
				else
				{
					_original = QueryString.Create(_parsedValues).ToString();
					HttpRequestFeature.QueryString = _original;
				}
			}
		}
	}

	public QueryFeature(IQueryCollection query)
	{
		if (query == null)
		{
			throw new ArgumentNullException("query");
		}
		_parsedValues = query;
	}

	public QueryFeature(IFeatureCollection features)
	{
		if (features == null)
		{
			throw new ArgumentNullException("features");
		}
		_features = new FeatureReferences<IHttpRequestFeature>(features);
	}
}
