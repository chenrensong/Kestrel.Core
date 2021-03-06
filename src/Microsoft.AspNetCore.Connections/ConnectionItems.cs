using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Connections;

public class ConnectionItems : IDictionary<object, object>, ICollection<KeyValuePair<object, object>>, IEnumerable<KeyValuePair<object, object>>, IEnumerable
{
	public IDictionary<object, object> Items { get; }

	object IDictionary<object, object>.this[object key]
	{
		get
		{
			if (Items.TryGetValue(key, out var value))
			{
				return value;
			}
			return null;
		}
		set
		{
			Items[key] = value;
		}
	}

	ICollection<object> IDictionary<object, object>.Keys => Items.Keys;

	ICollection<object> IDictionary<object, object>.Values => Items.Values;

	int ICollection<KeyValuePair<object, object>>.Count => Items.Count;

	bool ICollection<KeyValuePair<object, object>>.IsReadOnly => Items.IsReadOnly;

	public ConnectionItems()
		: this(new Dictionary<object, object>())
	{
	}

	public ConnectionItems(IDictionary<object, object> items)
	{
		Items = items;
	}

	void IDictionary<object, object>.Add(object key, object value)
	{
		Items.Add(key, value);
	}

	bool IDictionary<object, object>.ContainsKey(object key)
	{
		return Items.ContainsKey(key);
	}

	bool IDictionary<object, object>.Remove(object key)
	{
		return Items.Remove(key);
	}

	bool IDictionary<object, object>.TryGetValue(object key, out object value)
	{
		return Items.TryGetValue(key, out value);
	}

	void ICollection<KeyValuePair<object, object>>.Add(KeyValuePair<object, object> item)
	{
		Items.Add(item);
	}

	void ICollection<KeyValuePair<object, object>>.Clear()
	{
		Items.Clear();
	}

	bool ICollection<KeyValuePair<object, object>>.Contains(KeyValuePair<object, object> item)
	{
		return Items.Contains(item);
	}

	void ICollection<KeyValuePair<object, object>>.CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
	{
		Items.CopyTo(array, arrayIndex);
	}

	bool ICollection<KeyValuePair<object, object>>.Remove(KeyValuePair<object, object> item)
	{
		if (Items.TryGetValue(item.Key, out var value) && object.Equals(item.Value, value))
		{
			return Items.Remove(item.Key);
		}
		return false;
	}

	IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
	{
		return Items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Items.GetEnumerator();
	}
}
