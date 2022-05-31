using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Http;

public class FormCollection : IFormCollection, IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
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

	public static readonly FormCollection Empty = new FormCollection();

	private static readonly string[] EmptyKeys = Array.Empty<string>();

	private static readonly StringValues[] EmptyValues = Array.Empty<StringValues>();

	private static readonly Enumerator EmptyEnumerator = default(Enumerator);

	private static readonly IEnumerator<KeyValuePair<string, StringValues>> EmptyIEnumeratorType = EmptyEnumerator;

	private static readonly IEnumerator EmptyIEnumerator = EmptyEnumerator;

	private static IFormFileCollection EmptyFiles = new FormFileCollection();

	private IFormFileCollection _files;

	public IFormFileCollection Files
	{
		get
		{
			return _files ?? EmptyFiles;
		}
		private set
		{
			_files = value;
		}
	}

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
	}

	public int Count => Store?.Count ?? 0;

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

	private FormCollection()
	{
	}

	public FormCollection(Dictionary<string, StringValues> fields, IFormFileCollection files = null)
	{
		Store = fields;
		_files = files;
	}

	public bool ContainsKey(string key)
	{
		if (Store == null)
		{
			return false;
		}
		return Store.ContainsKey(key);
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
}
