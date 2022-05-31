using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.ObjectPool;

public class LeakTrackingObjectPool<T> : ObjectPool<T> where T : class
{
	private class Tracker : IDisposable
	{
		private readonly string _stack;

		private bool _disposed;

		public Tracker()
		{
			_stack = Environment.StackTrace;
		}

		public void Dispose()
		{
			_disposed = true;
			GC.SuppressFinalize(this);
		}

		~Tracker()
		{
			if (!_disposed)
			{
				_ = Environment.HasShutdownStarted;
			}
		}
	}

	private readonly ConditionalWeakTable<T, Tracker> _trackers = new ConditionalWeakTable<T, Tracker>();

	private readonly ObjectPool<T> _inner;

	public LeakTrackingObjectPool(ObjectPool<T> inner)
	{
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		_inner = inner;
	}

	public override T Get()
	{
		T val = _inner.Get();
		_trackers.Add(val, new Tracker());
		return val;
	}

	public override void Return(T obj)
	{
		if (_trackers.TryGetValue(obj, out var value))
		{
			_trackers.Remove(obj);
			value.Dispose();
		}
		_inner.Return(obj);
	}
}
