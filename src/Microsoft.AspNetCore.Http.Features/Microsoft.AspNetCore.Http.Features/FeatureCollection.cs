using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Http.Features;

public class FeatureCollection : IFeatureCollection, IEnumerable<KeyValuePair<Type, object>>, IEnumerable
{
	private class KeyComparer : IEqualityComparer<KeyValuePair<Type, object>>
	{
		public bool Equals(KeyValuePair<Type, object> x, KeyValuePair<Type, object> y)
		{
			return x.Key.Equals(y.Key);
		}

		public int GetHashCode(KeyValuePair<Type, object> obj)
		{
			return obj.Key.GetHashCode();
		}
	}

	private static KeyComparer FeatureKeyComparer = new KeyComparer();

	private readonly IFeatureCollection _defaults;

	private IDictionary<Type, object> _features;

	private volatile int _containerRevision;

	public virtual int Revision => _containerRevision + (_defaults?.Revision ?? 0);

	public bool IsReadOnly => false;

	public object this[Type key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (_features == null || !_features.TryGetValue(key, out var value))
			{
				return _defaults?[key];
			}
			return value;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (value == null)
			{
				if (_features != null && _features.Remove(key))
				{
					_containerRevision++;
				}
				return;
			}
			if (_features == null)
			{
				_features = new Dictionary<Type, object>();
			}
			_features[key] = value;
			_containerRevision++;
		}
	}

	public FeatureCollection()
	{
	}

	public FeatureCollection(IFeatureCollection defaults)
	{
		_defaults = defaults;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
	{
		if (_features != null)
		{
			foreach (KeyValuePair<Type, object> feature in _features)
			{
				yield return feature;
			}
		}
		if (_defaults == null)
		{
			yield break;
		}
		IEnumerable<KeyValuePair<Type, object>> enumerable;
		if (_features != null)
		{
			enumerable = _defaults.Except(_features, FeatureKeyComparer);
		}
		else
		{
			IEnumerable<KeyValuePair<Type, object>> defaults = _defaults;
			enumerable = defaults;
		}
		foreach (KeyValuePair<Type, object> item in enumerable)
		{
			yield return item;
		}
	}

	public TFeature Get<TFeature>()
	{
		return (TFeature)this[typeof(TFeature)];
	}

	public void Set<TFeature>(TFeature instance)
	{
		this[typeof(TFeature)] = instance;
	}
}
