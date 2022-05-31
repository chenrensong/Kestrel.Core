using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http.Internal;

public class RequestCookieCollection : IRequestCookieCollection, IEnumerable<KeyValuePair<string, string>>, IEnumerable
{
	public struct Enumerator : IEnumerator<KeyValuePair<string, string>>, IEnumerator, IDisposable
	{
		private Dictionary<string, string>.Enumerator _dictionaryEnumerator;

		private bool _notEmpty;

		public KeyValuePair<string, string> Current
		{
			get
			{
				if (_notEmpty)
				{
					KeyValuePair<string, string> current = _dictionaryEnumerator.Current;
					return new KeyValuePair<string, string>(current.Key, current.Value);
				}
				return default(KeyValuePair<string, string>);
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(Dictionary<string, string>.Enumerator dictionaryEnumerator)
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

		public void Reset()
		{
			if (_notEmpty)
			{
				((IEnumerator)_dictionaryEnumerator).Reset();
			}
		}
	}

	public static readonly RequestCookieCollection Empty = new RequestCookieCollection();

	private static readonly string[] EmptyKeys = Array.Empty<string>();

	private static readonly Enumerator EmptyEnumerator = default(Enumerator);

	private static readonly IEnumerator<KeyValuePair<string, string>> EmptyIEnumeratorType = EmptyEnumerator;

	private static readonly IEnumerator EmptyIEnumerator = EmptyEnumerator;

	private Dictionary<string, string> Store { get; set; }

	public string this[string key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (Store == null)
			{
				return null;
			}
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public int Count
	{
		get
		{
			if (Store == null)
			{
				return 0;
			}
			return Store.Count;
		}
	}

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

	public RequestCookieCollection()
	{
	}

	public RequestCookieCollection(Dictionary<string, string> store)
	{
		Store = store;
	}

	public RequestCookieCollection(int capacity)
	{
		Store = new Dictionary<string, string>(capacity, StringComparer.OrdinalIgnoreCase);
	}

	public static RequestCookieCollection Parse(IList<string> values)
	{
		if (values.Count == 0)
		{
			return Empty;
		}
		if (CookieHeaderValue.TryParseList(values, out var parsedValues))
		{
			if (parsedValues.Count == 0)
			{
				return Empty;
			}
			RequestCookieCollection requestCookieCollection = new RequestCookieCollection(parsedValues.Count);
			Dictionary<string, string> store = requestCookieCollection.Store;
			for (int i = 0; i < parsedValues.Count; i++)
			{
				CookieHeaderValue cookieHeaderValue = parsedValues[i];
				string key = Uri.UnescapeDataString(cookieHeaderValue.Name.Value);
				string text2 = (store[key] = Uri.UnescapeDataString(cookieHeaderValue.Value.Value));
			}
			return requestCookieCollection;
		}
		return Empty;
	}

	public bool ContainsKey(string key)
	{
		if (Store == null)
		{
			return false;
		}
		return Store.ContainsKey(key);
	}

	public bool TryGetValue(string key, out string value)
	{
		if (Store == null)
		{
			value = null;
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

	IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
	{
		if (Store == null || Store.Count == 0)
		{
			return EmptyIEnumeratorType;
		}
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Store == null || Store.Count == 0)
		{
			return EmptyIEnumerator;
		}
		return GetEnumerator();
	}
}
