using System;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.Http.Features;

public struct FeatureReferences<TCache>
{
	public TCache Cache;

	public IFeatureCollection Collection { get; private set; }

	public int Revision { get; private set; }

	public FeatureReferences(IFeatureCollection collection)
	{
		Collection = collection;
		Cache = default(TCache);
		Revision = collection.Revision;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TFeature Fetch<TFeature, TState>(ref TFeature cached, TState state, Func<TState, TFeature> factory) where TFeature : class
	{
		bool flush = false;
		int revision = Collection.Revision;
		if (Revision != revision)
		{
			cached = null;
			flush = true;
		}
		return cached ?? UpdateCached(ref cached, state, factory, revision, flush);
	}

	private TFeature UpdateCached<TFeature, TState>(ref TFeature cached, TState state, Func<TState, TFeature> factory, int revision, bool flush) where TFeature : class
	{
		if (flush)
		{
			Cache = default(TCache);
		}
		cached = Collection.Get<TFeature>();
		if (cached == null)
		{
			cached = factory(state);
			Collection.Set(cached);
			Revision = Collection.Revision;
		}
		else if (flush)
		{
			Revision = revision;
		}
		return cached;
	}

	public TFeature Fetch<TFeature>(ref TFeature cached, Func<IFeatureCollection, TFeature> factory) where TFeature : class
	{
		return Fetch(ref cached, Collection, factory);
	}
}
