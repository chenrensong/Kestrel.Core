using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http;

public class HeaderDictionary : IHeaderDictionary, IDictionary<string, StringValues>, ICollection<KeyValuePair<string, StringValues>>, IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
{
	public struct Enumerator : IEnumerator<KeyValuePair<string, StringValues>>, IEnumerator, IDisposable
	{
		private Dictionary<string, StringValues>.Enumerator _dictionaryEnumerator;

		private bool _notEmpty;

		public KeyValuePair<string, StringValues> Current
		{
			get
			{
				if (_notEmpty)
				{
					return _dictionaryEnumerator.Current;
				}
				return default(KeyValuePair<string, StringValues>);
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(Dictionary<string, StringValues>.Enumerator dictionaryEnumerator)
		{
			_dictionaryEnumerator = dictionaryEnumerator;
			_notEmpty = true;
		}

		public bool MoveNext()
		{
			if (_notEmpty)
			{
				return _dictionaryEnumerator.MoveNext();
			}
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			if (_notEmpty)
			{
				((IEnumerator)_dictionaryEnumerator).Reset();
			}
		}
	}

	private static readonly string[] EmptyKeys = Array.Empty<string>();

	private static readonly StringValues[] EmptyValues = Array.Empty<StringValues>();

	private static readonly Enumerator EmptyEnumerator = default(Enumerator);

	private static readonly IEnumerator<KeyValuePair<string, StringValues>> EmptyIEnumeratorType = EmptyEnumerator;

	private static readonly IEnumerator EmptyIEnumerator = EmptyEnumerator;

	private Dictionary<string, StringValues> Store { get; set; }

	public StringValues this[string key]
	{
		get
		{
			if (Store == null)
			{
				return StringValues.Empty;
			}
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			return StringValues.Empty;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			ThrowIfReadOnly();
			if (StringValues.IsNullOrEmpty(value))
			{
				Store?.Remove(key);
				return;
			}
			EnsureStore(1);
			Store[key] = value;
		}
	}

	StringValues IDictionary<string, StringValues>.this[string key]
	{
		get
		{
			return Store[key];
		}
		set
		{
			ThrowIfReadOnly();
			this[key] = value;
		}
	}

	public long? ContentLength
	{
		get
		{
			StringValues stringValues = this["Content-Length"];
			if (stringValues.Count == 1 && !string.IsNullOrEmpty(stringValues[0]) && HeaderUtilities.TryParseNonNegativeInt64(new StringSegment(stringValues[0]).Trim(), out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			ThrowIfReadOnly();
			if (value.HasValue)
			{
				this["Content-Length"] = HeaderUtilities.FormatNonNegativeInt64(value.Value);
			}
			else
			{
				Remove("Content-Length");
			}
		}
	}

	public int Count => Store?.Count ?? 0;

	public bool IsReadOnly { get; set; }

	public ICollection<string> Keys
	{
		get
		{
			if (Store == null)
			{
				return EmptyKeys;
			}
			return Store.Keys;
		}
	}

	public ICollection<StringValues> Values
	{
		get
		{
			if (Store == null)
			{
				return EmptyValues;
			}
			return Store.Values;
		}
	}

	public HeaderDictionary()
	{
	}

	public HeaderDictionary(Dictionary<string, StringValues> store)
	{
		Store = store;
	}

	public HeaderDictionary(int capacity)
	{
		EnsureStore(capacity);
	}

	private void EnsureStore(int capacity)
	{
		if (Store == null)
		{
			Store = new Dictionary<string, StringValues>(capacity, StringComparer.OrdinalIgnoreCase);
		}
	}

	public void Add(KeyValuePair<string, StringValues> item)
	{
		if (item.Key == null)
		{
			throw new ArgumentNullException("The key is null");
		}
		ThrowIfReadOnly();
		EnsureStore(1);
		Store.Add(item.Key, item.Value);
	}

	public void Add(string key, StringValues value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		ThrowIfReadOnly();
		EnsureStore(1);
		Store.Add(key, value);
	}

	public void Clear()
	{
		ThrowIfReadOnly();
		Store?.Clear();
	}

	public bool Contains(KeyValuePair<string, StringValues> item)
	{
		if (Store == null || !Store.TryGetValue(item.Key, out var value) || !StringValues.Equals(value, item.Value))
		{
			return false;
		}
		return true;
	}

	public bool ContainsKey(string key)
	{
		if (Store == null)
		{
			return false;
		}
		return Store.ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
	{
		if (Store == null)
		{
			return;
		}
		foreach (KeyValuePair<string, StringValues> item in Store)
		{
			KeyValuePair<string, StringValues> keyValuePair = (array[arrayIndex] = item);
			arrayIndex++;
		}
	}

	public bool Remove(KeyValuePair<string, StringValues> item)
	{
		ThrowIfReadOnly();
		if (Store == null)
		{
			return false;
		}
		if (Store.TryGetValue(item.Key, out var value) && StringValues.Equals(item.Value, value))
		{
			return Store.Remove(item.Key);
		}
		return false;
	}

	public bool Remove(string key)
	{
		ThrowIfReadOnly();
		if (Store == null)
		{
			return false;
		}
		return Store.Remove(key);
	}

	public bool TryGetValue(string key, out StringValues value)
	{
		if (Store == null)
		{
			value = default(StringValues);
			return false;
		}
		return Store.TryGetValue(key, out value);
	}

	public Enumerator GetEnumerator()
	{
		if (Store == null || Store.Count == 0)
		{
			return EmptyEnumerator;
		}
		return new Enumerator(Store.GetEnumerator());
	}

	IEnumerator<KeyValuePair<string, StringValues>> IEnumerable<KeyValuePair<string, StringValues>>.GetEnumerator()
	{
		if (Store == null || Store.Count == 0)
		{
			return EmptyIEnumeratorType;
		}
		return Store.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Store == null || Store.Count == 0)
		{
			return EmptyIEnumerator;
		}
		return Store.GetEnumerator();
	}

	private void ThrowIfReadOnly()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException("The response headers cannot be modified because the response has already started.");
		}
	}
}
